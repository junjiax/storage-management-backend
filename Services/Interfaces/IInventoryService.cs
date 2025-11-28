using dotnet_backend.DTOs.Inventory;

namespace dotnet_backend.Services
{
    public interface IInventoryService
    {
        Task<List<InventoryResponse>> GetInventoryListAsync();
        Task<InventoryResponse> AddInventoryItemAsync(InventoryRequest request);
        Task<InventoryResponse> UpdateInventoryItemAsync(int inventoryId, InventoryRequest request);
        Task<bool> DeleteInventoryItemAsync(int inventoryId);
        Task<InventoryResponse?> GetInventoryItemByIdAsync(int inventoryId);
        Task<bool> InventoryItemExistsAsync(int inventoryId);

    }
}
