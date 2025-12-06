
using dotnet_backend.DTOs.Payment;
using Microsoft.AspNetCore.Http;
namespace dotnet_backend.Interfaces
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(PaymentInformationDto model, string ipAddress);
        PaymentResponseDto PaymentExecute(IQueryCollection queryCollection);
    }
}
