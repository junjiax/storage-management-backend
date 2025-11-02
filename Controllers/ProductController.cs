using dotnet_backend.DTOs.Product;
using dotnet_backend.Services;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<ActionResult<List<ProductResponse>>> GetProducts()
        {
            var products = await _productService.GetProductListAsync();
            return Ok(products);
        }

        [HttpGet("{productId}")]
        public async Task<ActionResult<ProductResponse>> GetProductById(int productId)
        {
            var product = await _productService.GetProductItemByIdAsync(productId);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }

        [HttpPost]
        public async Task<ActionResult<ProductResponse>> AddProduct([FromBody] ProductRequest request)
        {
            var product = await _productService.AddProductItemAsync(request);
            return CreatedAtAction(nameof(GetProductById), new { productId = product.ProductId }, product);
        }

        [HttpPut("{productId}")]
        public async Task<ActionResult<ProductResponse>> UpdateProduct(int productId, [FromBody] ProductRequest request)
        {
            var updatedProduct = await _productService.UpdateProductItemAsync(productId, request);
            if (updatedProduct == null)
            {
                return NotFound();
            }
            return Ok(updatedProduct);
        }

        [HttpDelete("{productId}")]
        public async Task<IActionResult> DeleteProduct(int productId)
        {
            var success = await _productService.DeleteProductItemAsync(productId);
            if (!success)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}