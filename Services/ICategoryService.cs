using dotnet_backend.DTOs.Category;
namespace dotnet_backend.Services
{
    public interface ICategoryService
    {
        Task<List<CategoryResponse>> GetCategoryListAsync();
        Task<CategoryResponse?> GetCategoryByIdAsync(int categoryId);
        Task<CategoryResponse> AddCategoryItemAsync(CategoryRequest request);
        Task<CategoryResponse?> UpdateCategoryItemAsync(int categoryId, CategoryRequest request);
        Task<bool> DeleteCategoryItemAsync(int categoryId);
        Task<bool> CategoryExistsAsync(int categoryId);
    }
}





















