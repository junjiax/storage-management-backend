namespace dotnet_backend.DTOs.Payment
{
    public class CreatePaymentRequest
    {
        public int OrderId;
        public decimal Amount;
        public string? PaymentMethod;
    }
}
