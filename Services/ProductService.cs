using Microsoft.EntityFrameworkCore;
using dotnet_backend.Data;
using dotnet_backend.DTOs.Product;

namespace dotnet_backend.Services
{
    public class ProductService : IProductService
    {
        private readonly StoreDbContext _context;

        public ProductService(StoreDbContext context)
        {
            _context = context;
        }

        public async Task<List<ProductResponse>> GetProductListAsync()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Select(p => new ProductResponse
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    Price = p.Price,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category != null ? p.Category.CategoryName : string.Empty,
                    SupplierId = p.SupplierId,
                    Barcode = p.Barcode,
                    Unit = p.Unit
                })
                .ToListAsync();

            return products;
        }

        public async Task<ProductResponse> AddProductItemAsync(ProductRequest request)
        {
            var newProduct = new Models.Product
            {
                ProductName = request.ProductName,
                CategoryId = request.CategoryId,
                SupplierId = request.SupplierId,
                Barcode = request.Barcode,
                Price = request.Price,
                Unit = request.Unit
            };

            _context.Products.Add(newProduct);
            await _context.SaveChangesAsync();

            var category = await _context.Categories.FindAsync(newProduct.CategoryId);

            return new ProductResponse
            {
                ProductId = newProduct.ProductId,
                ProductName = newProduct.ProductName,
                CategoryId = newProduct.CategoryId,
                CategoryName = category != null ? category.CategoryName : string.Empty,
                SupplierId = newProduct.SupplierId,
                Barcode = newProduct.Barcode,
                Price = newProduct.Price,
                Unit = newProduct.Unit
            };
        }

        public async Task<ProductResponse> UpdateProductItemAsync(int productId, ProductRequest request)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                throw new KeyNotFoundException("Product not found");
            }

            product.ProductName = request.ProductName;
            product.CategoryId = request.CategoryId;
            product.SupplierId = request.SupplierId;
            product.Barcode = request.Barcode;
            product.Price = request.Price;
            product.Unit = request.Unit;

            await _context.SaveChangesAsync();

            var category = await _context.Categories.FindAsync(product.CategoryId);

            return new ProductResponse
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                CategoryId = product.CategoryId,
                CategoryName = category != null ? category.CategoryName : string.Empty,
                SupplierId = product.SupplierId,
                Barcode = product.Barcode,
                Price = product.Price,
                Unit = product.Unit
            };
        }

        public async Task<bool> DeleteProductItemAsync(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return false;
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ProductResponse?> GetProductItemByIdAsync(int productId)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductId == productId);

            if (product == null)
            {
                return null;
            }

            return new ProductResponse
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                CategoryId = product.CategoryId,
                CategoryName = product.Category != null ? product.Category.CategoryName : string.Empty,
                SupplierId = product.SupplierId,
                Barcode = product.Barcode,
                Price = product.Price,
                Unit = product.Unit
            };
        }

        public async Task<bool> ProductItemExistsAsync(int productId)
        {
            return await _context.Products.AnyAsync(p => p.ProductId == productId);
        }

    }
}
