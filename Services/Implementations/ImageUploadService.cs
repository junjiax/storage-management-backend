


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
         _cloudinary.Api.Secure = true;
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
            //Folder = "products", // Tự động tạo thư mục "products"
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
            ResourceType = ResourceType.Image
         };

         // (SỬA) 1. Lấy kết quả trả về
         DeletionResult deletionResult = await _cloudinary.DestroyAsync(deleteParams);
         Console.WriteLine($"Cloudinary deletion result: {deletionResult.Result}");
         // (SỬA) 2. Kiểm tra kết quả
         // "not found" (không tìm thấy) cũng có thể coi là thành công, 
         // vì dù sao file cũng không còn tồn tại.
         if (deletionResult.Result != "ok" && deletionResult.Result != "not found")
         {
            // Nếu kết quả không phải "ok" hoặc "not found",
            // nghĩa là đã có lỗi thực sự (ví dụ: API key sai, v.v.)
            throw new Exception($"Cloudinary delete error: {deletionResult.Error?.Message ?? "Unknown error"}");
         }

         // Nếu "ok" hoặc "not found" thì không làm gì cả, 
         // coi như đã xoá thành công.
      }
   }
}




