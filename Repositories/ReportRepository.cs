using dotnet_backend.Data;
using dotnet_backend.DTOs.Report;
using dotnet_backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace dotnet_backend.Repositories
{
   public class ReportRepository : IReportRepository
   {
      private readonly StoreDbContext _context;
      private readonly IConfiguration _configuration;
      public ReportRepository(StoreDbContext context, IConfiguration configuration)
      {
         _context = context;
         _configuration = configuration;
      }

      public async Task<RevenueReportResponse> GetFlexibleRevenueReportAsync(RevenueReportRequest request)
      {
         // 1. Bắt đầu với IQueryable<Payment>. 
         // Chúng ta dùng Include() để tải các dữ liệu liên quan mà ta cần lọc
         IQueryable<Payment> baseQuery = _context.Payments
             .Include(p => p.Order) // BAO GỒM BẢNG ORDERS
                 .ThenInclude(o => o.OrderItems); // TỪ ORDERS, BAO GỒM LUÔN ORDER_ITEMS

         // 2. Áp dụng các bộ lọc (filters) động

         // Lọc theo thời gian
         if (request.StartDate.HasValue)
         {
            baseQuery = baseQuery.Where(p => p.PaymentDate >= request.StartDate.Value);
         }
         if (request.EndDate.HasValue)
         {
            baseQuery = baseQuery.Where(p => p.PaymentDate < request.EndDate.Value.AddDays(1));
         }

         // Lọc theo nhân viên (dùng Order.UserId)
         if (request.EmployeeId.HasValue)
         {
            baseQuery = baseQuery.Where(p => p.Order.UserId == request.EmployeeId.Value);
         }

         // Lọc theo sản phẩm (dùng Order.OrderItems)
         if (request.ProductId.HasValue)
         {
            baseQuery = baseQuery.Where(p =>
                p.Order.OrderItems.Any(item => item.ProductId == request.ProductId.Value)
            );
         }

         // 4. Lấy danh sách payment (phân trang)
         var totalCount = await baseQuery.LongCountAsync();
         var paymentList = await baseQuery
             .OrderByDescending(p => p.PaymentDate)
             .Skip((request.PageNumber - 1) * request.PageSize)
             .Take(request.PageSize)
             .Select(p => new PaymentReportResponse
             {
                PaymentId = p.PaymentId,
                OrderId = p.OrderId,
                Amount = p.Amount,
                PaymentDate = p.PaymentDate,
                PaymentMethod = p.PaymentMethod
             })
             .ToListAsync();


         decimal totalRevenue;
         if (request.ProductId.HasValue)
         {
            // --- LOGIC MỚI: KHI LỌC THEO SẢN PHẨM ---
            // Chúng ta phải truy vấn từ OrderItems để lấy đúng Subtotal

            IQueryable<OrderItem> itemQuery = _context.OrderItems;

            // Áp dụng bộ lọc ProductId (bắt buộc)
            itemQuery = itemQuery.Where(item => item.ProductId == request.ProductId.Value);

            // Áp dụng các bộ lọc khác (Employee, Date)
            if (request.EmployeeId.HasValue)
            {
               itemQuery = itemQuery.Where(item => item.Order.UserId == request.EmployeeId.Value);
            }

            // Lọc theo ngày Payment của Order
            if (request.StartDate.HasValue)
            {
               itemQuery = itemQuery.Where(item =>
                   item.Order.Payments.Any(p => p.PaymentDate >= request.StartDate.Value)
               );
            }
            if (request.EndDate.HasValue)
            {
               itemQuery = itemQuery.Where(item =>
                   item.Order.Payments.Any(p => p.PaymentDate < request.EndDate.Value.AddDays(1))
               );
            }

            // Tính tổng Subtotal của CHỈ các item đã lọc
            totalRevenue = await itemQuery.SumAsync(item => item.Subtotal);
         }
         else
         {
            // --- LOGIC CŨ: KHI KHÔNG LỌC SẢN PHẨM ---
            // Tổng doanh thu là tổng của các Payment tìm được (từ baseQuery)
            totalRevenue = await baseQuery.SumAsync(p => p.Amount);
         }

         // 5. Đóng gói kết quả
         var paginatedResult = new PaginatedResult<PaymentReportResponse>
         {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize),
            Items = paymentList
         };

         var finalResponse = new RevenueReportResponse
         {
            TotalRevenue = totalRevenue, // Sử dụng totalRevenue đã được tính đúng
            Payments = paginatedResult
         };

         return finalResponse;
      }

      public async Task<InventoryReportReponse> GetLowStockReportAsync(InventoryReportRequest request)
      {
         // 1. Đọc ngưỡng
         int lowStockThreshold = _configuration.GetValue<int>("Reporting:LowStockThreshold", 30);
         var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

         // 2. Lọc
         IQueryable<Product> query = _context.Products
             .Include(p => p.Inventory)
             .Where(p => p.Inventory != null && p.Inventory.Quantity <= lowStockThreshold);

         // 3. Project sang DTO (Item)
         var dtoQuery = query.Select(p => new LowStockReportResponse
         {
            ProductId = p.ProductId,
            ProductName = p.ProductName,
            QuantityInStock = p.Inventory!.Quantity,

            SalesLast30Days = p.OrderItems
                    .Where(oi => oi.Order.OrderDate >= thirtyDaysAgo)
                    .Sum(oi => oi.Quantity) // EF Core sẽ dịch cái này
         });

         // 4. Phân trang
         var totalCount = await dtoQuery.LongCountAsync();
         var itemsFromDb = await dtoQuery
             .OrderBy(p => p.QuantityInStock)
             .Skip((request.PageNumber - 1) * request.PageSize)
             .Take(request.PageSize)
             .ToListAsync();


         var items = itemsFromDb.Select(p =>
         {
            p.SuggestedReorderQty = CalculateReorderSuggestion(p.SalesLast30Days, p.QuantityInStock);
            return p;
         }).ToList();


         // 5. Đóng gói PaginatedResult
         var paginatedResult = new PaginatedResult<LowStockReportResponse>
         {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize),
            Items = items
         };

         // 6. ĐÓNG GÓI VÀO RESPONSE CHÍNH (THEO YÊU CẦU MỚI)
         var finalResponse = new InventoryReportReponse
         {
            InventoryItems = paginatedResult
         };

         return finalResponse;

      }

      private static int CalculateReorderSuggestion(int salesLast30Days, int quantityInStock)
      {
         // Logic cơ bản: "Muốn nhập đủ hàng để bán trong 45 ngày tới"
         // (1.5 * Lượng bán 30 ngày)

         decimal dailySales = salesLast30Days / 30.0m;
         int desiredStockFor45Days = (int)Math.Ceiling(dailySales * 45);

         int reorderAmount = desiredStockFor45Days - quantityInStock;

         // Nếu đang đủ hàng (số âm) thì không cần nhập (trả về 0)
         return (reorderAmount > 0) ? reorderAmount : 0;
      }

      public async Task<PromotionReportResponse> GetPromotionSimpleReportAsync(PromotionReportRequest request)
      {
         var reportStart = new DateTime(request.Year, request.Month, 1);
         var reportEndExclusive = reportStart.AddMonths(1);

         var activePromoTask = await _context.Promotions
             .CountAsync(p =>
                 p.StartDate < reportEndExclusive &&
                 p.EndDate >= reportStart
             );

         var usedPromoTask = await _context.Orders
             .CountAsync(o =>
                 o.PromoId != null &&               
                 o.OrderDate >= reportStart &&     
                 o.OrderDate < reportEndExclusive
             );


         return new PromotionReportResponse
         {
            TotalPromotionsActiveInMonth =  activePromoTask,
            TotalPromotionsUsed = usedPromoTask
         };
      }

      public async Task<SimpleReportResponse> GetSimpleReportAsync(SimpleReportRequest request)
      {
         // === 1. ĐỊNH NGHĨA KỲ BÁO CÁO ===
         DateTime? currentStartDate = request.StartDate;
         DateTime? currentEndDateExclusive = request.EndDate?.AddDays(1);
         DateTime? prevStartDate = null;
         DateTime? prevEndDateExclusive = null;
         decimal totalPrevRevenue = 0;

         if (currentStartDate.HasValue && currentEndDateExclusive.HasValue)
         {
            var duration = currentEndDateExclusive.Value - currentStartDate.Value;
            prevEndDateExclusive = currentStartDate;
            prevStartDate = currentStartDate.Value.Add(-duration);
         }

         // === 2. TẠO QUERY KỲ NÀY ===
         IQueryable<Order> currentOrdersQuery = _context.Orders;
         if (currentStartDate.HasValue)
         {
            currentOrdersQuery = currentOrdersQuery.Where(o => o.OrderDate >= currentStartDate.Value);
         }
         if (currentEndDateExclusive.HasValue)
         {
            currentOrdersQuery = currentOrdersQuery.Where(o => o.OrderDate < currentEndDateExclusive.Value);
         }

         // === 3. TÍNH TOÁN KỲ NÀY (Chạy tuần tự, an toàn) ===

         // Await ngay lập tức
         decimal totalRevenue = await currentOrdersQuery.SumAsync(o => o.TotalAmount);

         // Await ngay lập tức
         int totalOrders = await currentOrdersQuery.CountAsync();

         // Lấy danh sách Customer ID (đã thực thi)
         var customerIdsInPeriod = await currentOrdersQuery
                                         .Where(o => o.CustomerId != null)
                                         .Select(o => o.CustomerId.Value)
                                         .Distinct()
                                         .ToListAsync(); // <-- Dùng ToListAsync()

         // === 4. TÍNH TOÁN KỲ TRƯỚC ===
         if (prevStartDate.HasValue)
         {
            IQueryable<Order> prevOrdersQuery = _context.Orders
                .Where(o =>
                    o.OrderDate >= prevStartDate.Value &&
                    o.OrderDate < prevEndDateExclusive.Value);

            // Await ngay lập tức
            totalPrevRevenue = await prevOrdersQuery.SumAsync(o => o.TotalAmount);
         }

         // === 5. TÍNH KHÁCH HÀNG MỚI ===
         int newCustomers = 0;
         if (currentStartDate.HasValue && customerIdsInPeriod.Any()) // Kiểm tra nếu có KH
         {
            // customerIdsInPeriod giờ là List<int>, không còn là IQueryable
            var firstOrderDates = _context.Orders
                .Where(o => o.CustomerId != null && customerIdsInPeriod.Contains(o.CustomerId.Value))
                .GroupBy(o => o.CustomerId.Value)
                .Select(g => new { FirstOrderDate = g.Min(o => o.OrderDate) });

            // Await ngay lập tức
            newCustomers = await firstOrderDates
                .Where(x =>
                    x.FirstOrderDate >= currentStartDate.Value &&
                    x.FirstOrderDate < currentEndDateExclusive.Value)
                .CountAsync();
         }

         // === 6. TÍNH TĂNG TRƯỞNG ===
         double growthRate = 0.0;
         if (totalPrevRevenue > 0)
         {
            growthRate = ((double)totalRevenue - (double)totalPrevRevenue) / (double)totalPrevRevenue * 100.0;
         }
         else if (totalRevenue > 0)
         {
            growthRate = 100.0;
         }

         // === 7. ĐÓNG GÓI KẾT QUẢ ===
         return new SimpleReportResponse
         {
            TotalRevenue = totalRevenue,
            TotalOrders = totalOrders,
            NewCustomers = newCustomers,
            GrowthRate = Math.Round(growthRate, 2)
         };
      }

      public async Task<RevenueByMothResponse> GetRevenueByMonthAsync(RevenueByMothRequest request)
      {
         // 1. Lấy tất cả dữ liệu có thật từ CSDL
         // Bằng cách dùng ToDictionary, chúng ta chỉ truy vấn 1 lần, rất hiệu quả
         var revenueByMonthDb = await _context.Orders
             .Where(o => o.OrderDate.Year == request.Year)
             .GroupBy(o => o.OrderDate.Month) // Nhóm theo số tháng (1, 2, 3...)
             .Select(g => new MonthlyRevenueData
             {
                Month = g.Key,
                Revenue = g.Sum(o => o.TotalAmount)
             })
             .ToDictionaryAsync(r => r.Month); // Chuyển thành { 1: data, 3: data, ... }

         // 2. Tạo danh sách 12 tháng chuẩn (để điền số 0)
         var fullYearRevenue = new List<MonthlyRevenueData>();

         for (int month = 1; month <= 12; month++)
         {
            // 3. Kiểm tra xem tháng 'month' có trong dữ liệu CSDL không
            if (revenueByMonthDb.TryGetValue(month, out var dataFromDb))
            {
               // Nếu có, thêm dữ liệu thật
               fullYearRevenue.Add(dataFromDb);
            }
            else
            {
               // Nếu không, thêm dữ liệu với doanh thu = 0
               fullYearRevenue.Add(new MonthlyRevenueData { Month = month, Revenue = 0 });
            }
         }

         // 4. Đóng gói kết quả
         return new RevenueByMothResponse
         {
            Year = request.Year,
            MonthlyRevenues = fullYearRevenue
         };
      }

   }


}
