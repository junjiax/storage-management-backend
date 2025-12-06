using Microsoft.EntityFrameworkCore;
using dotnet_backend.Data;
using dotnet_backend.DTOs.Category;
using dotnet_backend.Models;

namespace dotnet_backend.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly StoreDbContext _context;

        public CategoryService(StoreDbContext context)
        {
            _context = context;
        }

        public async Task<List<CategoryResponse>> GetCategoryListAsync()
        {
            return await _context.Categories
                .Select(c => new CategoryResponse
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName
                })
                .ToListAsync();
        }

        public async Task<CategoryResponse?> GetCategoryByIdAsync(int categoryId)
        {
            return await _context.Categories
                .Where(c => c.CategoryId == categoryId)
                .Select(c => new CategoryResponse
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName
                })
                .FirstOrDefaultAsync();
        }
        public async Task<CategoryResponse> AddCategoryItemAsync(CategoryRequest request)
        {
            var newCategory = new Category
            {
                CategoryName = request.CategoryName
            };

            _context.Categories.Add(newCategory);
            await _context.SaveChangesAsync();

            return new CategoryResponse
            {
                CategoryId = newCategory.CategoryId,
                CategoryName = newCategory.CategoryName
            };
        }

        public async Task<CategoryResponse?> UpdateCategoryItemAsync(int categoryId, CategoryRequest request)
        {
            var category = await _context.Categories.FindAsync(categoryId);
            if (category == null)
            {
                return null;
            }

            category.CategoryName = request.CategoryName;
            await _context.SaveChangesAsync();

            return new CategoryResponse
            {
                CategoryId = category.CategoryId,
                CategoryName = category.CategoryName
            };
        }

        public async Task<bool> DeleteCategoryItemAsync(int categoryId)
        {
            var category = await _context.Categories.FindAsync(categoryId);
            if (category == null)
            {
                return false;
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CategoryExistsAsync(int categoryId)
        {
            return await _context.Categories.AnyAsync(c => c.CategoryId == categoryId);
        }
    }
}
