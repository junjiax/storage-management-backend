

using dotnet_backend.DTOs.Common;
using dotnet_backend.DTOs.Payment;
using dotnet_backend.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace dotnet_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IVnPayService _vnPayService;

        public PaymentController(IVnPayService vnPayService)
        {
            _vnPayService = vnPayService;
        }

        // Tạo URL thanh toán
        [HttpPost("create-vnpay")]
        public IActionResult CreatePaymentUrlVnpay([FromBody] PaymentInformationDto model)
        {
            // Get IP address from HttpContext
            var ipAddress = GetIpAddress(HttpContext);
            var url = _vnPayService.CreatePaymentUrl(model, ipAddress);
            return Ok(ApiResponse<object>.Ok(new { paymentUrl = url }));

            // Nếu muốn redirect trực tiếp từ trình duyệt:
            // return Redirect(url);
        }

        // Callback sau khi thanh toán
        [HttpGet("vnpay-callback")]
        public IActionResult PaymentCallbackVnpay()
        {
            // Convert IQueryCollection to Dictionary
            var response = _vnPayService.PaymentExecute(Request.Query);
            return Ok(ApiResponse<PaymentResponseDto>.Ok(response));
        }

        private string GetIpAddress(HttpContext context)
        {
            var ipAddress = context.Connection.RemoteIpAddress?.ToString();
            if (string.IsNullOrEmpty(ipAddress))
            {
                ipAddress = context.Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',')[0]?.Trim();
            }
            if (string.IsNullOrEmpty(ipAddress))
            {
                ipAddress = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            }
            return ipAddress ?? "127.0.0.1";
        }
    }
}
