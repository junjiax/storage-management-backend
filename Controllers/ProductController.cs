using dotnet_backend.DTOs.Common;
using dotnet_backend.DTOs.Product;
using dotnet_backend.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace dotnet_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<ProductResponse>>>> GetProducts()
        {
            var products = await _productService.GetProductListAsync();
            return Ok(ApiResponse<List<ProductResponse>>.Ok(
                data: products,
                message: "Products retrieved successfully"
            ));
        }

        [HttpGet("category/{categoryId}")]
        public async Task<ActionResult<ApiResponse<List<ProductResponse>>>> GetProductsByCategory(int categoryId)
        {
            var products = await _productService.GetProductsByCategoryAsync(categoryId);
            return Ok(ApiResponse<List<ProductResponse>>.Ok(
                data: products,
                message: "Products filtered by category retrieved successfully"
            ));
        }

        [HttpGet("{productId}")]
        public async Task<ActionResult<ApiResponse<ProductResponse>>> GetProductById(int productId)
        {
            var product = await _productService.GetProductItemByIdAsync(productId);
            if (product == null)
            {
                return NotFound(ApiResponse<ProductResponse>.Fail("Product not found", statusCode: 404));
            }

            return Ok(ApiResponse<ProductResponse>.Ok(
                data: product,
                message: "Product retrieved successfully"
            ));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<ProductResponse>>> AddProduct([FromBody] ProductRequest request)
        {
            var product = await _productService.AddProductItemAsync(request);

            return CreatedAtAction(
                nameof(GetProductById),
                new { productId = product.ProductId },
                ApiResponse<ProductResponse>.Ok(
                    data: product,
                    message: "Product added successfully"
                )
            );
        }

        [HttpPut("{productId}")]
        public async Task<ActionResult<ApiResponse<ProductResponse>>> UpdateProduct(int productId, [FromBody] ProductRequest request)
        {
            var updatedProduct = await _productService.UpdateProductItemAsync(productId, request);
            if (updatedProduct == null)
            {
                return NotFound(ApiResponse<ProductResponse>.Fail("Product not found", statusCode: 404));
            }

            return Ok(ApiResponse<ProductResponse>.Ok(
                data: updatedProduct,
                message: "Product updated successfully"
            ));
        }

        [HttpDelete("{productId}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteProduct(int productId)
        {
            var success = await _productService.DeleteProductItemAsync(productId);
            if (!success)
            {
                return NotFound(ApiResponse<object>.Fail("Product not found", statusCode: 404));
            }

            return Ok(ApiResponse<object>.Ok(
                message: "Product deleted successfully"
            ));
        }
    }
}
