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
            //if (request.StartDate.HasValue)
            //{
            //   itemQuery = itemQuery.Where(item =>
            //       item.Order.Payments.Any(p => p.PaymentDate >= request.StartDate.Value)
            //   );
            //}
            //if (request.EndDate.HasValue)
            //{
            //   itemQuery = itemQuery.Where(item =>
            //       item.Order.Payments.Any(p => p.PaymentDate < request.EndDate.Value.AddDays(1))
            //   );
            //}

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

         // === 1. CHUYỂN ĐỔI YYYY-MM THÀNH START/END DATE ===
         var (startYear, startMonth) = SplitYearMonth(request.StartDate);
         var (endYear, endMonth) = SplitYearMonth(request.EndDate);
         // Ngày đầu tháng
         DateTime currentStartDate = new DateTime(startYear, startMonth, 1);
         // Ngày cuối tháng (đúng 28, 29, 30, 31)
         DateTime currentEndDate = new DateTime(
             endYear,
             endMonth,
             DateTime.DaysInMonth(endYear, endMonth)
         );

         // Dùng Exclusive để dễ query
         DateTime currentEndDateExclusive = currentEndDate.AddDays(1);

         // === 2. TÍNH KỲ TRƯỚC ===
         TimeSpan duration = currentEndDateExclusive - currentStartDate;

         DateTime prevEndExclusive = currentStartDate;
         DateTime prevStartDate = currentStartDate - duration;
         // === 3. QUERY KỲ HIỆN TẠI ===
         IQueryable<Order> currentOrdersQuery = _context.Orders
             .Where(o => o.OrderDate >= currentStartDate &&
                         o.OrderDate < currentEndDateExclusive &&
                o.Status == "paid");

         decimal totalRevenue = await currentOrdersQuery.SumAsync(o => o.TotalAmount);
         int totalOrders = await currentOrdersQuery.CountAsync();

         // Danh sách khách hàng trong kỳ
         var customerIdsInPeriod = await currentOrdersQuery
             .Where(o => o.CustomerId != null)
             .Select(o => o.CustomerId.Value)
             .Distinct()
             .ToListAsync();

         // === 4. QUERY KỲ TRƯỚC ===
         decimal totalPrevRevenue = 0;

         IQueryable<Order> prevOrdersQuery = _context.Orders
             .Where(o => o.OrderDate >= prevStartDate &&
                         o.OrderDate < prevEndExclusive &&
                o.Status == "paid");

         totalPrevRevenue = await prevOrdersQuery.SumAsync(o => o.TotalAmount);

         // === 5. TÍNH KHÁCH HÀNG MỚI (First Order trong khoảng) ===
         int newCustomers = 0;

         if (customerIdsInPeriod.Any())
         {
            var firstOrderDates = _context.Orders
                .Where(o => o.CustomerId != null && o.Status == "paid" && customerIdsInPeriod.Contains(o.CustomerId.Value))
                .GroupBy(o => o.CustomerId.Value)
                .Select(g => new { FirstOrderDate = g.Min(o => o.OrderDate) });

            newCustomers = await firstOrderDates
                .Where(x => x.FirstOrderDate >= currentStartDate &&
                            x.FirstOrderDate < currentEndDateExclusive)
                .CountAsync();
         }

         // === 6. TÍNH TĂNG TRƯỞNG ===
         double growthRate = 0;

         if (totalPrevRevenue > 0)
         {
            growthRate = ((double)totalRevenue - (double)totalPrevRevenue)
                         / (double)totalPrevRevenue * 100.0;
         }
         else if (totalRevenue > 0)
         {
            growthRate = 100.0;
         }

         // === 7. TRẢ KẾT QUẢ ===
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
         // Split YYYY-MM
         var (startYear, startMonth) = SplitYearMonth(request.StartDate);
         var (endYear, endMonth) = SplitYearMonth(request.EndDate);

         var startDate = new DateTime(startYear, startMonth, 1);
         var endDate = new DateTime(endYear, endMonth, DateTime.DaysInMonth(endYear, endMonth));

         Console.WriteLine(">>>>>>>>>>>>"+startDate + endDate);
         // Query DB
         var revenueByMonthDb = await _context.Orders
             .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
             .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
             .Select(g => new MonthlyRevenueData
             {
                Month = g.Key.Month,
                Revenue = g.Sum(o => o.TotalAmount),
                Orders = g.Count()
             })
             .ToDictionaryAsync(x => x.Month);

         // Build list month range
         var months = Enumerable.Range(startMonth, endMonth - startMonth + 1).ToList();

         var result = new List<MonthlyRevenueData>();
         foreach (var m in months)
         {
            result.Add(revenueByMonthDb.TryGetValue(m, out var data)
                ? data
                : new MonthlyRevenueData { Month = m, Revenue = 0, Orders = 0 });
         }

         return new RevenueByMothResponse
         {
            Year = startYear,
            MonthlyRevenues = result
         };
      }

      public async Task<RatioByCategoryResponse> GetRatioByCategoryAsync(RatioByCategoryRequest request)
      {
         // Split YYYY-MM
         var (startYear, startMonth) = SplitYearMonth(request.StartDate);
         var (endYear, endMonth) = SplitYearMonth(request.EndDate);

         var startDate = new DateTime(startYear, startMonth, 1);
         var endDate = new DateTime(endYear, endMonth, DateTime.DaysInMonth(endYear, endMonth));

         Console.WriteLine(">>>>>>>>>>>>" + startDate + endDate);

         var query = _context.OrderItems
        // Include các bảng liên quan để truy cập thuộc tính
        .Include(oi => oi.Order)
        .Include(oi => oi.Product)
        .ThenInclude(p => p.Category)
        // Điều kiện lọc
        .Where(oi => oi.Order.OrderDate >= startDate &&
                     oi.Order.OrderDate <= endDate)
        // (Tuỳ chọn) Bạn nên lọc thêm Status nếu chỉ muốn tính đơn hàng thành công
        // .Where(oi => oi.Order.Status == "completed") 

        // Gom nhóm theo Tên danh mục
        // Sử dụng toán tử ?. và ?? để xử lý trường hợp Product chưa có Category (null)
        .GroupBy(oi => oi.Product.Category.CategoryName)

        // Chọn ra dữ liệu cần thiết
        .Select(g => new RatioByCategoryData
        {
           Type = g.Key ?? "Chưa phân loại", // Tên danh mục (CategoryName)
           Value = g.Sum(oi => oi.Quantity)  // Tổng số lượng bán ra
        });

         // 3. Thực thi truy vấn và trả về kết quả
         var resultData = await query.ToListAsync();

         return new RatioByCategoryResponse
         {
            Ratios = resultData
         };

      }

      private (int Year, int Month) SplitYearMonth(string ym)
      {
         var parts = ym.Split('-');
         if (parts.Length != 2)
            throw new Exception("Định dạng tháng phải là YYYY-MM");

         return (int.Parse(parts[0]), int.Parse(parts[1]));
      }
      public async Task<OrdersByDayResponse> GetOrdersByDayAsync(OrdersByDayRequest request)
      {
         var query = _context.Orders
             .Where(o => o.OrderDate.Year == request.Year && o.OrderDate.Month == request.Month);

         var dbData = await query
             .GroupBy(o => o.OrderDate.Day)
             .Select(g => new
             {
                //Date = new DateTime(request.Year, request.Month, g.Key),
                Date = g.Key,
                Orders = g.Count(),     
                
             })
             .OrderBy(s => s.Date)
             .ToListAsync();

         int daysInMonth = DateTime.DaysInMonth(request.Year, request.Month);

         var result = new List<DailyOrdersData>();
         for (int day = 1; day <= daysInMonth; day++)
         {
            var record = dbData.FirstOrDefault(x => x.Date == day);

            var dailyData = new DailyOrdersData
            {
               Date = new DateTime(request.Year, request.Month, day),
               Orders = record != null ? record.Orders : 0
            };

            result.Add(dailyData);
         }
         var response = new OrdersByDayResponse
         {
            DailyOrders = result,
            Month = request.Month,
            Year = request.Year
         };

         return response;

      }



   }


}