using dotnet_backend.DTOs.Order;
using dotnet_backend.Models;

namespace dotnet_backend.Services.Interfaces
{
    public interface IOrderService
    {
        Task<Order> CreateOrderAsync(CreateOrderRequest request);
        Task<Order?> GetOrderByIdAsync(int Id);
        Task<List<Order>> GetAllOrdersAsync();
    }
}