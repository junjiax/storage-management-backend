using dotnet_backend.DTOs.Product; 
using dotnet_backend.DTOs.Common;
using dotnet_backend.Models;
using dotnet_backend.Services;  

namespace dotnet_backend.Services
{
    public interface IProductService
    {
        Task<List<ProductResponse>> GetProductListAsync();
        Task<ProductResponse> AddProductItemAsync(ProductRequest request);
        Task<ProductResponse> UpdateProductItemAsync(int productId, ProductRequest request);
        Task<bool> DeleteProductItemAsync(int productId);
        Task<ProductResponse?> GetProductItemByIdAsync(int productId);
        Task<bool> ProductItemExistsAsync(int productId);

    }
}