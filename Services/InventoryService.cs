using Microsoft.EntityFrameworkCore;
using dotnet_backend.Data;
using dotnet_backend.DTOs.Inventory;

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
                    Quantity = i.Quantity,
                    UpdatedAt = i.UpdatedAt
                })
                .ToListAsync();

            return items;
        }
    }
}
