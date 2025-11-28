using dotnet_backend.DTOs.Common;
using dotnet_backend.DTOs.Order;
using dotnet_backend.Models;

namespace dotnet_backend.Services.Interfaces
{
    public interface IOrderService
    {
        Task<Order> CreateOrderAsync(CreateOrderRequest request);
        Task<Order?> GetOrderByIdAsync(int id);
        Task<List<Order>> GetAllOrdersAsync();
        Task UpdateOrderStatusAndInventoryAsync(int id);
        Task<ApiResponse<byte[]>> ExportOrderToPdfAsync(int id);
        Task<ApiResponse<string>> SendInvoiceEmailAsync(byte[] pdfBytes, int orderId);

    }   
}