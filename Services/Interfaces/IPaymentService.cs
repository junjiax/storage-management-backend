using System.Threading.Tasks;
using dotnet_backend.DTOs.Payment;
using dotnet_backend.Models;

namespace dotnet_backend.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<Payment> CreatePaymentAsync(CreatePaymentRequest request);
    }
}
