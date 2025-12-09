using dotnet_backend.Data;
using dotnet_backend.DTOs.Product;
using dotnet_backend.Models;
using Microsoft.EntityFrameworkCore;

namespace dotnet_backend.Services
{
    public class ProductService : IProductService
    {
        private readonly StoreDbContext _context;

         private readonly IImageUploadService _imageUploadService;
      public ProductService(StoreDbContext context, IImageUploadService imageUploadService)
      {
         _context = context;
         _imageUploadService = imageUploadService;
      }

      public async Task<List<ProductResponse>> GetProductListAsync()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .Select(p => new ProductResponse
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    Price = p.Price,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category != null ? p.Category.CategoryName : string.Empty,
                    SupplierId = p.SupplierId,
                    SupplierName = p.Supplier != null ? p.Supplier.Name : string.Empty,
                    Barcode = p.Barcode,
                    ProductImg = p.ProductImg,
                   Unit = p.Unit,
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
                Unit = product.Unit,
                ProductImg = product.ProductImg
            };
        }

        public async Task<bool> ProductItemExistsAsync(int productId)
        {
            return await _context.Products.AnyAsync(p => p.ProductId == productId);
        }




      public async Task<ProductResponse> AddProductItemWithImageAsync(ProductWithUploadImgRequest request)
      {
         var newProduct = new Models.Product
         {
            ProductName = request.ProductName,
            CategoryId = request.CategoryId,
            SupplierId = request.SupplierId,
            Barcode = request.Barcode,
            Price = request.Price != null ? request.Price : 0,
            Unit = request.Unit,
            ProductImg = "",
            ProductPublicId = null
         };

         _context.Products.Add(newProduct);
         await _context.SaveChangesAsync();


         if (request.ImageFile != null && request.ImageFile.Length > 0)
         {
            // Key (tên file) trên Cloudinary
            string publicId = $"products/{newProduct.ProductId}/{Guid.NewGuid()}";

            string imageUrl = await _imageUploadService.UploadImageAsync(request.ImageFile, publicId);

            // Cập nhật cả URL và PublicId
            newProduct.ProductImg = imageUrl;
            newProduct.ProductPublicId = publicId; // <-- LƯU LẠI KEY
            await _context.SaveChangesAsync(); // Lưu lần 2
         }



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
            Unit = newProduct.Unit,
            ProductImg = newProduct.ProductImg
         };


      }

      public async Task<ProductResponse> UpdateProductItemWithImageAsync(int productId, ProductWithUploadImgRequest request)
      {
         var product = await _context.Products.FindAsync(productId);
         if (product == null)
         {
            throw new KeyNotFoundException("Product not found");
         }

         // 🟢 Chỉ update nếu có giá trị
         if (request.ProductName != null && request.ProductName.Length > 0)
            product.ProductName = request.ProductName;

         if (request.CategoryId.HasValue)
            product.CategoryId = request.CategoryId.Value;

         if (request.SupplierId.HasValue)
            product.SupplierId = request.SupplierId.Value;

         if (request.Barcode != null)
            product.Barcode = request.Barcode;

         if (request.Price != null)
            product.Price = request.Price;

         if (request.Unit != null)
            product.Unit = request.Unit;


         if (request.ImageFile != null && request.ImageFile.Length > 0)
         {
            // 2. Tải ảnh MỚI
            string newPublicId = $"products/{product.ProductId}/{Guid.NewGuid()}";
            string newImageUrl = await _imageUploadService.UploadImageAsync(request.ImageFile, newPublicId);

            string oldPublicId = product.ProductPublicId;

            // 3. Cập nhật thông tin ảnh MỚI vào DB
            product.ProductImg = newImageUrl;
            product.ProductPublicId = newPublicId;

            if (!string.IsNullOrEmpty(oldPublicId))
            {
               _ = Task.Run(async () =>
               {
                  try
                  {
                     await _imageUploadService.DeleteImageAsync(oldPublicId);
                  }
                  catch (Exception)
                  {
                     // Ghi log lỗi lại
                     // _logger.LogWarning(ex, "Lỗi xóa ảnh nền Cloudinary");
                  }
               });
            }
         }


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
            Unit = product.Unit,
            ProductImg = product.ProductImg
         };



      }
   }
}
