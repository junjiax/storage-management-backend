using System.Text.Json.Serialization;

namespace dotnet_backend.DTOs.Category
{
    public class CreateCategoryRequest
    {
        [JsonPropertyName("categoryName")]
        public string CategoryName { get; set; } = string.Empty;
    }

    public class UpdateCategoryRequest
    {
        [JsonPropertyName("categoryName")]
        public string CategoryName { get; set; } = string.Empty;
    }

    public class CategoryResponse
    {
        [JsonPropertyName("categoryId")]
        public int CategoryId { get; set; }

        [JsonPropertyName("categoryName")]
        public string CategoryName { get; set; } = string.Empty;
    }
}