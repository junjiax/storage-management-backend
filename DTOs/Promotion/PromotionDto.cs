using System.Text.Json.Serialization;

namespace dotnet_backend.DTOs.Promotion
{
    public class CreatePromotionRequest
    {
        [JsonPropertyName("promoCode")]
        public string PromoCode { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("discountType")]
        public string DiscountType { get; set; } = string.Empty;

        [JsonPropertyName("discountValue")]
        public decimal DiscountValue { get; set; }

        [JsonPropertyName("startDate")]
        public DateTime StartDate { get; set; }

        [JsonPropertyName("endDate")]
        public DateTime EndDate { get; set; }

        [JsonPropertyName("minOrderAmount")]
        public decimal MinOrderAmount { get; set; }

        [JsonPropertyName("usageLimit")]
        public int UsageLimit { get; set; }
        
        [JsonPropertyName("status")]
        public string Status { get; set; } = "active"; // <--- Thêm property này
    }

    public class UpdatePromotionRequest
    {
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("discountValue")]
        public decimal? DiscountValue { get; set; }
        [JsonPropertyName("discountType")]
        public string? DiscountType { get; set; } // <--- Thêm nếu muốn update loại giảm
        [JsonPropertyName("startDate")]
        public DateTime? StartDate { get; set; } // <--- Thêm nếu muốn update ngày bắt đầu

        [JsonPropertyName("endDate")]
        public DateTime? EndDate { get; set; }

        [JsonPropertyName("minOrderAmount")]
        public decimal? MinOrderAmount { get; set; }

        [JsonPropertyName("usageLimit")]
        public int? UsageLimit { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }
    }

    public class PromotionResponse
    {
        [JsonPropertyName("promoId")]
        public int PromoId { get; set; }

        [JsonPropertyName("promoCode")]
        public string PromoCode { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("discountType")]
        public string DiscountType { get; set; } = string.Empty;

        [JsonPropertyName("discountValue")]
        public decimal DiscountValue { get; set; }

        [JsonPropertyName("startDate")]
        public DateTime StartDate { get; set; }

        [JsonPropertyName("endDate")]
        public DateTime EndDate { get; set; }

        [JsonPropertyName("minOrderAmount")]
        public decimal MinOrderAmount { get; set; }

        [JsonPropertyName("usageLimit")]
        public int UsageLimit { get; set; }

        [JsonPropertyName("usedCount")]
        public int UsedCount { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;
    }
}