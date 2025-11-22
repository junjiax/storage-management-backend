


using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using dotnet_backend.Libraries;
using Microsoft.Extensions.Options;

namespace dotnet_backend.Services
{
   public class ImageUploadService : IImageUploadService
   {
      private readonly Cloudinary _cloudinary;

      public ImageUploadService(IOptions<CloudinarySettings> config)
      {
         var account = new Account(
             config.Value.CloudName,
             config.Value.ApiKey,
             config.Value.ApiSecret
         );
         _cloudinary = new Cloudinary(account);
         _cloudinary.Api.Secure = true; // Luôn dùng HTTPS
      }

      public async Task<string> UploadImageAsync(IFormFile file, string publicId)
      {
         // Kiểm tra file
         if (file == null || file.Length == 0)
         {
            // Trả về null hoặc ném exception
            return null;
         }

         // Mở stream
         await using var stream = file.OpenReadStream();

         // Các tham số tải lên
         var uploadParams = new ImageUploadParams()
         {
            File = new FileDescription(file.FileName, stream),
            PublicId = publicId, // Key của bạn
            Folder = "products", // Tự động tạo thư mục "products"
            Overwrite = true // Ghi đè file nếu có
         };

         // Tải lên
         var uploadResult = await _cloudinary.UploadAsync(uploadParams);

         // Xử lý lỗi (nếu có)
         if (uploadResult.Error != null)
         {
            throw new System.Exception($"Lỗi khi tải ảnh lên Cloudinary: {uploadResult.Error.Message}");
         }

         // Trả về URL
         return uploadResult.SecureUrl.ToString();
      }

      public async Task DeleteImageAsync(string publicId)
      {
         // Nếu publicId rỗng thì không làm gì
         if (string.IsNullOrEmpty(publicId)) return;

         var deleteParams = new DeletionParams(publicId)
         {
            ResourceType = ResourceType.Image // Chỉ định rõ là xóa ảnh
         };

         // Gửi yêu cầu xóa đến Cloudinary
         await _cloudinary.DestroyAsync(deleteParams);
      }
   }
}




