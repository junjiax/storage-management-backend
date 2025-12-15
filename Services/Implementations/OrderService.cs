using dotnet_backend.Data;
using dotnet_backend.DTOs.Inventory;
using dotnet_backend.DTOs.Order;
using dotnet_backend.Models;
using dotnet_backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using MimeKit;
using MailKit.Security;
using MailKit.Net.Smtp;
using System.ComponentModel.DataAnnotations;
using dotnet_backend.DTOs.Common;
using RabbitMQ.Client;
using System.Text;
using Newtonsoft.Json;
using dotnet_backend.RabbitMQ;



namespace dotnet_backend.Services.Implementations
{

    public class OrderService : IOrderService
    {
        private readonly StoreDbContext _context;
        private readonly IInventoryService _inventoryService;

        public OrderService(StoreDbContext context, IInventoryService inventoryService)
        {
            _context = context;
            _inventoryService = inventoryService;
        }

        public async Task<Order> CreateOrderAsync(CreateOrderRequest request)
        {

            // Lấy danh sách sản phẩm
            var productIds = request.Items.Select(i => i.ProductId).ToList();

            var products = await _context.Products
                .Where(p => productIds.Contains(p.ProductId))
                .ToListAsync();

            if (products.Count != request.Items.Count)
                throw new ArgumentException("Some products not found.");


            var order = new Order
            {
                CustomerId = request.CustomerId,
                UserId = request.UserId,
                PromoId = request.PromoId,
                OrderDate = DateTime.UtcNow,
                Status = "pending",
                DiscountAmount = 0
            };

            // Tính tổng tiền
            decimal totalAmount = 0;

            foreach (var item in request.Items)
            {
                var product = products.First(p => p.ProductId == item.ProductId);
                decimal subtotal = product.Price * item.Quantity;
                totalAmount += subtotal;

                order.OrderItems.Add(new OrderItem
                {
                    ProductId = product.ProductId,
                    Quantity = item.Quantity,
                    Price = product.Price,
                    Subtotal = subtotal,
                    Order = order
                });
            }

            Promotion? promo = null;
            if (request.PromoId != null)
            {
                promo = await _context.Promotions
                    .FirstOrDefaultAsync(p => p.PromoId == request.PromoId);
                if (promo!.DiscountType == "percent")
                {
                    order.DiscountAmount = totalAmount * promo!.DiscountValue / 100;
                    order.TotalAmount = totalAmount - order.DiscountAmount;

                }
                else if (promo!.DiscountType == "fixed")
                {
                    order.DiscountAmount = promo!.DiscountValue;
                    order.TotalAmount = totalAmount - order.DiscountAmount;
                }

            }
            else
            {
                order.TotalAmount = totalAmount;
            }

            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();

            return order;
        }

        public async Task UpdateOrderStatusAndInventoryAsync(int id)
        {
            var order = await GetOrderByIdAsync(id);
            if (order != null)
            {
                order.Status = "paid";
                foreach (var item in order.OrderItems)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        var inventory = await _context.Inventory.FindAsync(item.ProductId);
                        if (inventory != null)
                        {
                            await _inventoryService.UpdateInventoryItemAsync(
                                inventory.InventoryId,
                                new InventoryRequest
                                {
                                    ProductId = item.ProductId,
                                    Quantity = inventory.Quantity - item.Quantity
                                }
                            );
                        }

                    }
                }
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Order?> GetOrderByIdAsync(int orderId)
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.User)
                .Include(o => o.Promotion)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
        }

        public async Task<List<Order>> GetAllOrdersAsync()
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.User)
                .Include(o => o.Promotion)
                .Include(o => o.OrderItems)
                .Include(o => o.Payment)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<ApiResponse<byte[]>> ExportOrderToPdfAsync(int id)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            var order = await GetOrderByIdAsync(id);
            if (order == null)
                return ApiResponse<byte[]>.Fail($"Order with id {id} not found.", 404);

            var email = order.Customer?.Email;
            if (string.IsNullOrWhiteSpace(email))
                return ApiResponse<byte[]>.Fail("Customer does not have an email.", 400);

            var emailValidator = new EmailAddressAttribute();
            if (!emailValidator.IsValid(email))
                return ApiResponse<byte[]>.Fail("Customer email format is invalid.", 400);

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.ContinuousSize(width: 324);
                    page.Margin(20);

                    page.Header().Text($"HÓA ĐƠN BÁN HÀNG #{order.OrderId}")
                        .FontSize(18).Bold().AlignCenter().FontFamily("Arial");

                    page.Content().Column(col =>
                    {
                        col.Spacing(10);

                        // Thông tin khách hàng
                        col.Item().Column(column =>
                        {
                            column.Item().Row(row =>
                            {
                                row.RelativeItem().Text("Khách hàng:").FontFamily("Arial").Bold();
                                row.RelativeItem().AlignMiddle().Text(order.Customer?.Name ?? "N/A").FontFamily("Arial");
                            });

                            column.Item().Row(row =>
                            {
                                row.RelativeItem().Text("Ngày đặt:").FontFamily("Arial").Bold();
                                row.RelativeItem().AlignMiddle().Text(order.OrderDate.ToLocalTime().ToString("dd/MM/yyyy HH:mm")).FontFamily("Arial");
                            });

                            column.Item().Row(row =>
                            {
                                row.RelativeItem().Text("Nhân viên:").FontFamily("Arial").Bold();
                                row.RelativeItem().AlignMiddle().Text(order.User?.FullName ?? "N/A").FontFamily("Arial");
                            });

                            if (order.Promotion != null)
                            {
                                column.Item().Row(row =>
                                {
                                    row.RelativeItem().Text("Mã giảm giá:").FontFamily("Arial").Bold();
                                    row.RelativeItem().AlignMiddle().Text(order.Promotion.PromoCode).FontFamily("Arial");
                                });
                            }
                        });

                        col.Item().LineHorizontal(1);

                        // Bảng sản phẩm
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3); // Tên sản phẩm
                                columns.RelativeColumn(1); // Số lượng
                                columns.RelativeColumn(2); // Đơn giá
                                columns.RelativeColumn(2); // Thành tiền
                            });

                            // Header
                            table.Header(header =>
                            {
                                header.Cell().Text("Sản phẩm").Bold().FontFamily("Arial");
                                header.Cell().Text("SL").Bold();
                                header.Cell().Text("Đơn giá").Bold().FontFamily("Arial");
                                header.Cell().Text("Thành tiền").Bold().FontFamily("Arial");
                            });

                            // Rows
                            foreach (var item in order.OrderItems)
                            {
                                table.Cell().Text(item.Product?.ProductName ?? "").FontFamily("Arial");
                                table.Cell().Text(item.Quantity.ToString()).FontFamily("Arial");
                                table.Cell().Text($"{item.Price:N0} đ").FontFamily("Arial");
                                table.Cell().Text($"{item.Subtotal:N0} đ").FontFamily("Arial");
                            }
                        });

                        col.Item().LineHorizontal(1);

                        // Tổng cộng
                        col.Item().AlignRight().Column(total =>
                        {
                            total.Spacing(4);
                            total.Item().Text($"Tổng tiền hàng: {order.OrderItems.Sum(i => i.Subtotal):N0} đ").FontFamily("Arial");
                            if (order.DiscountAmount > 0)
                                total.Item().Text($"Giảm giá: -{order.DiscountAmount:N0} đ").FontFamily("Arial");
                            total.Item().Text($"Thành tiền: {order.TotalAmount:N0} đ").Bold().FontSize(14).FontFamily("Arial");
                        });

                        col.Item().LineHorizontal(1);

                        col.Item().Text("Cảm ơn quý khách đã mua hàng!").AlignCenter().Italic().FontFamily("Arial");
                    });
                });
            });

            byte[] pdfBytes = document.GeneratePdf();
            return ApiResponse<byte[]>.Ok(pdfBytes);
        }

        public async Task<ApiResponse<string>> QueueInvoiceEmailAsync(byte[] pdf, int orderId, string email)
        {
            var job = new EmailJob
            {
                OrderId = orderId,
                Email = email,
                PdfBytes = pdf
            };

            var publisher = new RabbitPublisher();
            await publisher.PublishAsync("email_queue", JsonConvert.SerializeObject(job));

            return ApiResponse<string>.Ok("Email job queued successfully.");
        }

        public async Task<List<Order>> SearchOrdersAsync(
            string? keyword,
            string? status,
            DateTime? fromDate,
            DateTime? toDate,
            string sortOrder
        )
        {
            IQueryable<Order> query = _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.User)
                .Include(o => o.Promotion)
                .Include(o => o.OrderItems)
                .Include(o => o.Payment);

            // Search theo keyword (Customer / User)
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(o =>
                    o.Customer != null && o.Customer.Name.Contains(keyword));
            }

            // Filter theo status
            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(o => o.Status == status);
            }

            // Filter từ ngày
            if (fromDate.HasValue)
            {
                query = query.Where(o => o.OrderDate >= fromDate.Value.Date);
            }

            // Filter đến ngày
            if (toDate.HasValue)
            {
                var endDate = toDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(o => o.OrderDate <= endDate);
            }

            // Sort theo ngày đặt
            query = sortOrder == "asc"
                ? query.OrderBy(o => o.OrderDate)
                : query.OrderByDescending(o => o.OrderDate);

            return await query.ToListAsync();
        }

        public async Task<List<Order>> GetOrdersByCustomerIdAsync(int customerId)
        {
            return await _context.Orders
                .Where(o => o.CustomerId == customerId)
                .Include(o => o.Customer)
                .Include(o => o.User)
                .Include(o => o.Promotion)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.Payment)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

    }
}