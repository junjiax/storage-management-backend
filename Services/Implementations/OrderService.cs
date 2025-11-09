using dotnet_backend.Data;
using dotnet_backend.DTOs;
using dotnet_backend.DTOs.Inventory;
using dotnet_backend.DTOs.Order;
using dotnet_backend.Models;
using dotnet_backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

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
    }
    
}