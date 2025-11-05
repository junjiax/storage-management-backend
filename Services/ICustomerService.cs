using dotnet_backend.DTOs.Customer;

namespace dotnet_backend.Services
{
    public interface ICustomerService
    {
        Task<List<CustomerResponse>> GetCustomerListAsync();
        Task<CustomerResponse?> GetCustomerByIdAsync(int customerId);
        Task<CustomerResponse> AddCustomerItemAsync(CreateCustomerRequest request); // Thay CustomerRequest
        Task<CustomerResponse?> UpdateCustomerItemAsync(int customerId, UpdateCustomerRequest request); // Thay CustomerRequest
        Task<bool> DeleteCustomerItemAsync(int customerId);
        Task<bool> CustomerExistsAsync(int customerId);
    }
}
