using System.Text.Json.Serialization;

namespace dotnet_backend.DTOs.Product
{
    public class ProductRequest
    {
        [JsonPropertyName("categoryId")]
        public int? CategoryId { get; set; }

        [JsonPropertyName("supplierId")]
        public int? SupplierId { get; set; }

        [JsonPropertyName("productName")]
        public string ProductName { get; set; } = string.Empty;

        [JsonPropertyName("barcode")]
        public string? Barcode { get; set; }

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("unit")]
        public string Unit { get; set; } = "pcs";
    }

    public class UpdateProductRequest
    {
        [JsonPropertyName("categoryId")]
        public int? CategoryId { get; set; }

        [JsonPropertyName("supplierId")]
        public int? SupplierId { get; set; }

        [JsonPropertyName("productName")]
        public string? ProductName { get; set; }

        [JsonPropertyName("barcode")]
        public string? Barcode { get; set; }

        [JsonPropertyName("price")]
        public decimal? Price { get; set; }

        [JsonPropertyName("unit")]
        public string? Unit { get; set; }
    }

    public class ProductResponse
    {
        [JsonPropertyName("productId")]
        public int ProductId { get; set; }

        [JsonPropertyName("categoryId")]
        public int? CategoryId { get; set; }

        [JsonPropertyName("categoryName")]
        public string? CategoryName { get; set; }

        [JsonPropertyName("supplierId")]
        public int? SupplierId { get; set; }

        [JsonPropertyName("supplierName")]
        public string? SupplierName { get; set; }

        [JsonPropertyName("productName")]
        public string ProductName { get; set; } = string.Empty;

        [JsonPropertyName("barcode")]
        public string? Barcode { get; set; }

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("unit")]
        public string Unit { get; set; } = string.Empty;


        // (THÊM DÒNG NÀY)
        [JsonPropertyName("productImg")]
        public string? ProductImg { get; set; } // <-- Thêm trường ảnh




        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("currentStock")]
        public int? CurrentStock { get; set; }
    }


    public class ProductWithUploadImgRequest
    {
        public int? CategoryId { get; set; }

        public int? SupplierId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public string? Barcode { get; set; }

        public decimal Price { get; set; }

        public string Unit { get; set; } = "pcs";

        public IFormFile? ImageFile { get; set; }
    }
}