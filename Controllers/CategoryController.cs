using dotnet_backend.DTOs.Category;
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
            return Ok(categories);
        }

        [HttpGet("{categoryId}")]
        public async Task<ActionResult<CategoryResponse>> GetCategoryById(int categoryId)
        {
            var category = await _categoryService.GetCategoryByIdAsync(categoryId);
            if (category == null)
            {
                return NotFound();
            }
            return Ok(category);
        }

        [HttpPost]
        public async Task<ActionResult<CategoryResponse>> AddCategory([FromBody] CategoryRequest request)
        {
            var category = await _categoryService.AddCategoryItemAsync(request);
            return CreatedAtAction(nameof(GetCategoryById), new { categoryId = category.CategoryId }, category);
        }

        [HttpPut("{categoryId}")]
        public async Task<ActionResult<CategoryResponse>> UpdateCategory(int categoryId, [FromBody] CategoryRequest request)
        {
            var updatedCategory = await _categoryService.UpdateCategoryItemAsync(categoryId, request);
            if (updatedCategory == null)
            {
                return NotFound();
            }
            return Ok(updatedCategory);
        }

        [HttpDelete("{categoryId}")]
        public async Task<IActionResult> DeleteCategory(int categoryId)
        {
            var success = await _categoryService.DeleteCategoryItemAsync(categoryId);
            if (!success)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}