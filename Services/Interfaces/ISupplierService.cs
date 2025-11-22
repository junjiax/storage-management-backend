using dotnet_backend.DTOs.Supplier;

namespace dotnet_backend.Services
{
    public interface ISupplierService
    {
        Task<List<SupplierResponse>> GetSupplierListAsync();
        Task<SupplierResponse?> GetSupplierByIdAsync(int supplierId);
        Task<SupplierResponse> AddSupplierItemAsync(SupplierRequest request);
        Task<SupplierResponse?> UpdateSupplierItemAsync(int supplierId, SupplierRequest request);
        Task<bool> DeleteSupplierItemAsync(int supplierId);
        Task<bool> SupplierExistsAsync(int supplierId);
    }
}
