using System.Text.Json.Serialization;

namespace dotnet_backend.DTOs.Inventory
{
    public class InventoryRequest
    {
        [JsonPropertyName("productId")]
        public int ProductId { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }
    }

    public class InventoryResponse
    {
        [JsonPropertyName("inventoryId")]
        public int InventoryId { get; set; }

        [JsonPropertyName("productId")]
        public int ProductId { get; set; }

        [JsonPropertyName("productName")]
        public string ProductName { get; set; } = string.Empty;

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("updatedAt")]
        public DateTime UpdatedAt { get; set; }

        [JsonPropertyName("productImg")]
        public string ProductImg { get; set; } = string.Empty;

        [JsonPropertyName("productPublicId")]
        public string ProductPublicId { get; set; } = string.Empty;
    }

    public class InventoryLogDto
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }              // "nhập hàng" | "bán hàng"
        
        [JsonPropertyName("date")]
        public string Date { get; set; }              // dd/MM/yyyy
        
        [JsonPropertyName("quantity")]
        public int? Quantity { get; set; }          // "+num" hoặc "num"
        
        [JsonPropertyName("orderId")]
        public int? OrderId { get; set; }
        
        [JsonPropertyName("quantitySold")]
        public int? QuantitySold { get; set; }
        
        [JsonPropertyName("stockRemaining")]
        public int? StockRemaining { get; set; }
    }


}

