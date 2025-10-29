using System.Text.Json.Serialization;

namespace dotnet_backend.DTOs.Payment
{
    public class CreatePaymentRequest
    {
        [JsonPropertyName("orderId")]
        public int OrderId { get; set; }

        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        [JsonPropertyName("paymentMethod")]
        public string PaymentMethod { get; set; } = "cash";
    }

    public class PaymentResponse
    {
        [JsonPropertyName("paymentId")]
        public int PaymentId { get; set; }

        [JsonPropertyName("orderId")]
        public int OrderId { get; set; }

        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        [JsonPropertyName("paymentMethod")]
        public string PaymentMethod { get; set; } = string.Empty;

        [JsonPropertyName("paymentDate")]
        public DateTime PaymentDate { get; set; }
    }
}