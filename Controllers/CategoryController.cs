using dotnet_backend.DTOs.Category;
using dotnet_backend.DTOs.Common;
using dotnet_backend.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace dotnet_backend.Controllers

{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<ActionResult<List<CategoryResponse>>> GetCategories()
        {
            var categories = await _categoryService.GetCategoryListAsync();
            return Ok(ApiResponse<IEnumerable<CategoryResponse>>.Ok(categories));
        }

        [HttpGet("{categoryId}")]
        public async Task<ActionResult<CategoryResponse>> GetCategoryById(int categoryId)
        {
            var category = await _categoryService.GetCategoryByIdAsync(categoryId);
            if (category == null)
            {
                return NotFound();
            }
            return Ok(ApiResponse<CategoryResponse>.Ok(category));
        }

        [HttpPost]
        public async Task<ActionResult<CategoryResponse>> AddCategory([FromBody] CategoryRequest request)
        {
            var category = await _categoryService.AddCategoryItemAsync(request);
            return CreatedAtAction(nameof(GetCategoryById), new { categoryId = category.CategoryId }, ApiResponse<CategoryResponse>.Ok(category, "Created category.", 201));
        }

        [HttpPut("{categoryId}")]
        public async Task<ActionResult<CategoryResponse>> UpdateCategory(int categoryId, [FromBody] CategoryRequest request)
        {
            var updatedCategory = await _categoryService.UpdateCategoryItemAsync(categoryId, request);
            if (updatedCategory == null)
            {
                return NotFound();
            }
            return Ok(ApiResponse<CategoryResponse>.Ok(updatedCategory,"Updated category."));
        }

        [HttpDelete("{categoryId}")]
        public async Task<IActionResult> DeleteCategory(int categoryId)
        {
            var success = await _categoryService.DeleteCategoryItemAsync(categoryId);
            if (!success)
            {
                return NotFound();
            }
            return Ok(ApiResponse.Ok("Category is deleted."));
        }
    }
}