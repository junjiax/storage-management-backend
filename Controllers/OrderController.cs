

using dotnet_backend.DTOs.Common;
using dotnet_backend.DTOs.Order;
using dotnet_backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace dotnet_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            try
            {
                var order = await _orderService.CreateOrderAsync(request);

                var response = new OrderResponse
                {
                    OrderId = order.OrderId,
                    CustomerId = order.CustomerId,
                    UserId = order.UserId,

                    PromoId = order.PromoId,
                    OrderDate = order.OrderDate,
                    Status = order.Status,
                    TotalAmount = order.TotalAmount,
                    DiscountAmount = order.DiscountAmount,
                    Items = order.OrderItems.Select(oi => new OrderItemResponse
                    {
                        OrderItemId = oi.OrderItemId,
                        ProductId = oi.ProductId,
                        ProductName = oi.Product?.ProductName ?? "",
                        Quantity = oi.Quantity,
                        Price = oi.Price,
                        Subtotal = oi.Subtotal
                    }).ToList(),
                };

                return Ok(ApiResponse<OrderResponse>.Ok(
                    data: response,
                    message: "Order created successfully"
                ));
            }
            catch (ArgumentException ex)
            {
                var errors = new Dictionary<string, string[]>
                {
                    { "order", new[] { ex.Message } }
                };

                return BadRequest(ApiResponse<OrderResponse>.Fail(
                    message: "Invalid order data",
                    statusCode: 400,
                    errors: errors
                ));
            }
            catch (Exception ex)
            {
                var errors = new Dictionary<string, string[]>
                {
                    { "exception", new[] { ex.Message } }
                };

                return StatusCode(500, ApiResponse<OrderResponse>.Fail(
                    message: "Internal server error",
                    statusCode: 500,
                    errors: errors
                ));
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> a(int id)
        {
            try
            {
                await _orderService.UpdateOrderStatusAndInventoryAsync(id);

                return Ok(ApiResponse<string>.Ok(
                    data: $"Order {id} status updated to 'paid' and inventory adjusted successfully.",
                    message: "Order status updated successfully"
                ));
            }
            catch (ArgumentException ex)
            {
                var errors = new Dictionary<string, string[]>
        {
            { "order", new[] { ex.Message } }
        };

                return BadRequest(ApiResponse<string>.Fail(
                    message: "Invalid order data",
                    statusCode: 400,
                    errors: errors
                ));
            }
            catch (Exception ex)
            {
                var errors = new Dictionary<string, string[]>
        {
            { "exception", new[] { ex.Message } }
        };

                return StatusCode(500, ApiResponse<string>.Fail(
                    message: "Internal server error",
                    statusCode: 500,
                    errors: errors
                ));
            }
        }


        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);

            if (order == null)
            {
                return NotFound(ApiResponse<OrderResponse>.Fail(
                    message: $"Order with id {id} not found",
                    statusCode: 404
                ));
            }

            var response = MapOrderToResponse(order);

            return Ok(ApiResponse<OrderResponse>.Ok(
                data: response,
                message: "Order retrieved successfully"
            ));
        }

        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _orderService.GetAllOrdersAsync();

            var response = orders.Select(MapOrderToResponse).ToList();

            return Ok(ApiResponse<List<OrderResponse>>.Ok(
                data: response,
                message: "Orders retrieved successfully"
            ));
        }

        private static OrderResponse MapOrderToResponse(dotnet_backend.Models.Order order)
        {
            return new OrderResponse
            {
                OrderId = order.OrderId,
                CustomerId = order.CustomerId,
                CustomerName = order.Customer?.Name,
                UserId = order.UserId,
                UserName = order.User?.FullName,
                PromoId = order.PromoId,
                PromoCode = order.Promotion?.PromoCode,
                OrderDate = order.OrderDate,
                Status = order.Status,
                TotalAmount = order.TotalAmount,
                DiscountAmount = order.DiscountAmount,

                Items = order.OrderItems.Select(oi => new OrderItemResponse
                {
                    OrderItemId = oi.OrderItemId,
                    ProductId = oi.ProductId,
                    ProductName = oi.Product?.ProductName ?? "",
                    Quantity = oi.Quantity,
                    Price = oi.Price,
                    Subtotal = oi.Subtotal
                }).ToList(),

                Payment = order.Payment is null ? null : new OrderPaymentResponse
                {
                    PaymentId = order.Payment.PaymentId,
                    PaymentMethod = order.Payment.PaymentMethod,
                    Amount = order.Payment.Amount,
                    PaymentDate = order.Payment.PaymentDate
                }
            };
        }
        
        [HttpGet("{id:int}/export-pdf")]
        public async Task<IActionResult> ExportOrderToPdf(int id)
        {
            try
            {
                var pdfBytes = await _orderService.ExportOrderToPdfAsync(id);
                // 🔹 Trả file trực tiếp, không wrap trong ApiResponse
                return File(pdfBytes, "application/pdf", $"Order_{id}.pdf");
            }
            catch (ArgumentException ex)
            {
                // 🔹 Trả NotFound trực tiếp
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // 🔹 Trả 500 trực tiếp
                return StatusCode(500, new { message = "Failed to generate PDF", detail = ex.Message });
            }
        }

    }
}