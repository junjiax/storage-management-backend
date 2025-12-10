using Microsoft.EntityFrameworkCore;
using dotnet_backend.Data;
using dotnet_backend.DTOs.Inventory;
using dotnet_backend.Models;

namespace dotnet_backend.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly StoreDbContext _context;

        public InventoryService(StoreDbContext context)
        {
            _context = context;
        }

        public async Task<List<InventoryResponse>> GetInventoryListAsync()
        {
            var items = await _context.Inventory
                .Include(i => i.Product)
                .OrderByDescending(i => i.UpdatedAt)
                .Select(i => new InventoryResponse
                {
                    InventoryId = i.InventoryId,
                    ProductId = i.ProductId,
                    ProductName = i.Product != null ? i.Product.ProductName : string.Empty,
                    ProductImg = i.Product != null ? i.Product.ProductImg : string.Empty,
                    ProductPublicId = i.Product != null ? i.Product.ProductPublicId : string.Empty,
                    Quantity = i.Quantity,
                    UpdatedAt = i.UpdatedAt
                })
                .ToListAsync();

            return items;
        }

        public async Task<InventoryResponse> AddInventoryItemAsync(InventoryRequest request)
        {
            var newItem = new Models.Inventory
            {
                ProductId = request.ProductId,
                Quantity = request.Quantity,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Inventory.Add(newItem);
            await _context.SaveChangesAsync();

            var product = await _context.Products.FindAsync(request.ProductId);

            return new InventoryResponse
            {
                InventoryId = newItem.InventoryId,
                ProductId = newItem.ProductId,
                ProductName = product != null ? product.ProductName : string.Empty,
                Quantity = newItem.Quantity,
                UpdatedAt = newItem.UpdatedAt
            };
        }

        public async Task<InventoryResponse> UpdateInventoryItemAsync(int inventoryId, InventoryRequest request)
        {
            var item = await _context.Inventory.FindAsync(inventoryId);
            if (item == null)
            {
                throw new KeyNotFoundException("Inventory item not found");
            }

            item.ProductId = request.ProductId;
            item.Quantity += request.Quantity;
            item.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var product = await _context.Products.FindAsync(request.ProductId);

            return new InventoryResponse
            {
                InventoryId = item.InventoryId,
                ProductId = item.ProductId,
                ProductName = product != null ? product.ProductName : string.Empty,
                Quantity = item.Quantity,
                UpdatedAt = item.UpdatedAt
            };
        }

        public async Task<bool> DeleteInventoryItemAsync(int inventoryId)
        {
            var item = await _context.Inventory.FindAsync(inventoryId);
            if (item == null)
            {
                return false;
            }

            _context.Inventory.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<InventoryResponse?> GetInventoryItemByIdAsync(int inventoryId)
        {
            var item = await _context.Inventory
                .Include(i => i.Product)
                .FirstOrDefaultAsync(i => i.InventoryId == inventoryId);

            if (item == null)
            {
                return null;
            }

            return new InventoryResponse
            {
                InventoryId = item.InventoryId,
                ProductId = item.ProductId,
                ProductName = item.Product != null ? item.Product.ProductName : string.Empty,
                Quantity = item.Quantity,
                UpdatedAt = item.UpdatedAt
            };
        }

        public async Task<bool> InventoryItemExistsAsync(int inventoryId)
        {
            return await _context.Inventory.AnyAsync(i => i.InventoryId == inventoryId);
        }

        public async Task<List<InventoryLogDto>> GetProductLogAsync(int productId)
        {
            var logs = new List<InventoryLogDto>();

            // Lấy log nhập hàng
            var inventoryUpdates = await _context.Inventory
                .Where(i => i.ProductId == productId)
                .OrderBy(i => i.UpdatedAt)
                .ToListAsync();

            // Lấy log bán hàng
            var sales = await _context.OrderItems
                .Where(oi => oi.ProductId == productId)
                .Join(_context.Orders.Where(o => o.Status == "paid"),
                      oi => oi.OrderId,
                      o => o.OrderId,
                      (oi, o) => new
                      {
                          o.OrderId,
                          o.OrderDate,
                          oi.Quantity
                      })
                .OrderBy(x => x.OrderDate)
                .ToListAsync();

            // Tính tồn kho theo thời gian
            int stock = 0;

            // Đưa inventory update vào stock trước (vì phải cộng vào trước khi bán)
            foreach (var inv in inventoryUpdates.OrderBy(i => i.UpdatedAt))
            {
                stock += inv.Quantity; // tăng số lượng nhập

                logs.Add(new InventoryLogDto
                {
                    Type = "nhập hàng",
                    Date = inv.UpdatedAt.ToString("dd/MM/yyyy"),
                    Quantity = inv.Quantity,
                    OrderId = null,
                    QuantitySold = null,
                    StockRemaining = stock
                });
            }

            // Tính theo timeline bán hàng
            foreach (var item in sales)
            {
                stock -= item.Quantity; // giảm số lượng bán

                logs.Add(new InventoryLogDto
                {
                    Type = "bán hàng",
                    Date = item.OrderDate.ToString("dd/MM/yyyy"),
                    Quantity = item.Quantity,
                    OrderId = item.OrderId,
                    QuantitySold = item.Quantity,
                    StockRemaining = stock
                });
            }

            // Gom chung và sắp xếp theo ngày
            logs = logs
                .OrderBy(l => DateTime.ParseExact(l.Date, "dd/MM/yyyy", null))
                .ToList();

            return logs;
        }

    }
}
