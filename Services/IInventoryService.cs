using dotnet_backend.DTOs.Inventory;

namespace dotnet_backend.Services
{
    public interface IInventoryService
    {
        Task<List<InventoryResponse>> GetInventoryListAsync();
    }
}
