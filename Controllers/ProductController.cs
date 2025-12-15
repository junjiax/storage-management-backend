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
        private const long MAX_FILE_SIZE = 5242880;
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

        [HttpPost("with-image")]
        public async Task<ActionResult<ApiResponse<ProductResponse>>> AddProductWithImageFile([FromBody] ProductWithUploadImgRequest request)
        {
            var product = await _productService.AddProductItemWithImageAsync(request);

            return CreatedAtAction(
                nameof(GetProductById),
                new { productId = product.ProductId },
                ApiResponse<ProductResponse>.Ok(
                    data: product,
                    message: "Product added successfully"
                )
            );
        }

        [HttpPost("upload")]
        public async Task<ActionResult<ApiResponse<ProductResponse>>> AddProductWithUpload(
    [FromForm] ProductWithUploadImgRequest request)
        {
            if (request.ImageFile != null)
            {
                if (request.ImageFile.Length == 0)
                {
                    return BadRequest(ApiResponse<ProductResponse>.Fail("Image file is empty", 400));
                }

                if (request.ImageFile.Length > MAX_FILE_SIZE)
                {
                    return BadRequest(ApiResponse<ProductResponse>.Fail("Image file size exceeds 5 MB", 400));
                }
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<ProductResponse>.Fail("Invalid data", 400));
            }

            try
            {
                // Gọi hàm service xử lý upload
                var product = await _productService.AddProductItemWithImageAsync(request);

                return CreatedAtAction(
                    nameof(GetProductById),
                    new { productId = product.ProductId },
                    ApiResponse<ProductResponse>.Ok(
                        data: product,
                        message: "Product added successfully (with image)"
                    )
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ProductResponse>.Fail($"Internal server error: {ex.Message}", 500));
            }
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

        [HttpPut("{productId}/upload")]
        public async Task<ActionResult<ApiResponse<ProductResponse>>> UpdateProductWithUpload(
            int productId,
            [FromForm] ProductWithUploadImgRequest request) // <-- Dùng [FromForm]
        {
            if (request.ImageFile != null)
            {
                if (request.ImageFile.Length == 0)
                {
                    return BadRequest(ApiResponse<ProductResponse>.Fail("Image file is empty", 400));
                }

                if (request.ImageFile.Length > MAX_FILE_SIZE)
                {
                    return BadRequest(ApiResponse<ProductResponse>.Fail("Image file size exceeds 5 MB", 400));
                }
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<ProductResponse>.Fail("Invalid data", 400));
            }

            try
            {
                // Gọi hàm service xử lý upload và xóa ảnh cũ
                var updatedProduct = await _productService.UpdateProductItemWithImageAsync(productId, request);

                return Ok(ApiResponse<ProductResponse>.Ok(
                    data: updatedProduct,
                    message: "Product updated successfully (with image)"
                ));
            }
            catch (KeyNotFoundException ex) // Bắt lỗi 404
            {
                return NotFound(ApiResponse<ProductResponse>.Fail(ex.Message, 404));
            }
            catch (Exception ex) // Bắt lỗi 500
            {
                return StatusCode(500, ApiResponse<ProductResponse>.Fail($"Internal server error: {ex.Message}", 500));
            }
        }
    }
}