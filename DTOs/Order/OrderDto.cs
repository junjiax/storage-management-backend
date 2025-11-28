using System.Text.Json.Serialization;

namespace dotnet_backend.DTOs.Order
{
    public class CreateOrderRequest
    {
        [JsonPropertyName("customerId")]
        public int? CustomerId { get; set; }

        [JsonPropertyName("userId")]
        public int? UserId { get; set; }

        [JsonPropertyName("promoId")]
        public int? PromoId { get; set; }

        [JsonPropertyName("items")]
        public List<OrderItemRequest> Items { get; set; } = new();
    }

    public class OrderItemRequest
    {
        [JsonPropertyName("productId")]
        public int ProductId { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        // [JsonPropertyName("price")]
        // public decimal Price { get; set; }
    }

    public class UpdateOrderStatusRequest
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;
    }

    public class OrderResponse
    {
        [JsonPropertyName("orderId")]
        public int OrderId { get; set; }

        [JsonPropertyName("customerId")]
        public int? CustomerId { get; set; }

        [JsonPropertyName("customerName")]
        public string? CustomerName { get; set; }

        [JsonPropertyName("userId")]
        public int? UserId { get; set; }

        [JsonPropertyName("userName")]
        public string? UserName { get; set; }

        [JsonPropertyName("promoId")]
        public int? PromoId { get; set; }

        [JsonPropertyName("promoCode")]
        public string? PromoCode { get; set; }

        [JsonPropertyName("orderDate")]
        public DateTime OrderDate { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("totalAmount")]
        public decimal TotalAmount { get; set; }

        [JsonPropertyName("discountAmount")]
        public decimal DiscountAmount { get; set; }

        [JsonPropertyName("items")]
        public List<OrderItemResponse> Items { get; set; } = new();

        [JsonPropertyName("payment")]
        public OrderPaymentResponse? Payment { get; set; }
    }

    public class OrderItemResponse
    {
        [JsonPropertyName("orderItemId")]
        public int OrderItemId { get; set; }

        [JsonPropertyName("productId")]
        public int ProductId { get; set; }

        [JsonPropertyName("productName")]
        public string ProductName { get; set; } = string.Empty;

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("subtotal")]
        public decimal Subtotal { get; set; }
    }

    public class OrderPaymentResponse
    {
        [JsonPropertyName("paymentId")]
        public int PaymentId { get; set; }

        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        [JsonPropertyName("paymentMethod")]
        public string? PaymentMethod { get; set; }

        [JsonPropertyName("paymentDate")]
        public DateTime PaymentDate { get; set; }
    }
}