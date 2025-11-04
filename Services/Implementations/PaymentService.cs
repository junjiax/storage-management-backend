using dotnet_backend.Data;
using dotnet_backend.DTOs.Payment;
using dotnet_backend.Models;
using dotnet_backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace dotnet_backend.Services.Implementations
{
    public class PaymentService : IPaymentService
    {
        private readonly StoreDbContext _context;

        public PaymentService(StoreDbContext context)
        {
            _context = context;
        }

        public async Task<Payment> CreatePaymentAsync(CreatePaymentRequest request)
        {
            var orderExists = await _context.Orders.AnyAsync(o => o.OrderId == request.OrderId);
            if (!orderExists)
            {
                throw new ArgumentException($"Order with ID {request.OrderId} not found.");
            }

            var payment = new Payment
            {
                OrderId = request.OrderId,
                Amount = request.Amount,
                PaymentMethod = request.PaymentMethod,
                PaymentDate = DateTime.UtcNow
            };

            _context.Payments.Add(payment);
            var rows = await _context.SaveChangesAsync();

            return payment;
        }
    }
}
