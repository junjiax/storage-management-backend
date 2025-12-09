namespace dotnet_backend.Services
{
   public interface IImageUploadService
   {
      Task<string> UploadImageAsync(IFormFile file, string publicId);
      Task DeleteImageAsync(string publicId);
   }
}
