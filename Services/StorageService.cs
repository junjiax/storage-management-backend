namespace dotnet_backend.Services
{
    public class StorageService : IStorageService
    {
        public string GetStatus() => "Storage service is running";
    }
}
