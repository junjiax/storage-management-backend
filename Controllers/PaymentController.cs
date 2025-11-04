

using dotnet_backend.DTOs.Common;
using dotnet_backend.DTOs.Payment;
using dotnet_backend.Interfaces;
using dotnet_backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace dotnet_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IVnPayService _vnPayService;
        private readonly IOrderService _orderService;

        private readonly IPaymentService _paymentService;

        public PaymentController(IVnPayService vnPayService, IOrderService orderService, IPaymentService paymentService)
        {
            _vnPayService = vnPayService;
            _orderService = orderService;
            _paymentService = paymentService;
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
        public async Task<IActionResult> PaymentCallbackVnpay()
        {
            // Convert IQueryCollection to Dictionary
            var response = _vnPayService.PaymentExecute(Request.Query);

            if (response.Success)
            {
                await _orderService.UpdateOrderStatusAsync(int.Parse(response.OrderId));
                var createPaymentRequest = new CreatePaymentRequest
                {
                    OrderId = int.Parse(response.OrderId),
                    Amount = response.Amount,
                    PaymentMethod = "bank_transfer",
                };

                await _paymentService.CreatePaymentAsync(createPaymentRequest);
            }

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
