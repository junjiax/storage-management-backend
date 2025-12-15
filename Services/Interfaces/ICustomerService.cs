using dotnet_backend.DTOs.Customer;

namespace dotnet_backend.Services
{
    public interface ICustomerService
    {
        Task<List<CustomerResponse>> GetCustomerListAsync();
        Task<CustomerResponse?> GetCustomerByIdAsync(int customerId);
        Task<CustomerResponse> AddCustomerItemAsync(CreateCustomerRequest request);
        Task<CustomerResponse?> UpdateCustomerItemAsync(int customerId, UpdateCustomerRequest request); 
        Task<bool> DeleteCustomerItemAsync(int customerId);
        Task<bool> CustomerExistsAsync(int customerId);
        Task<List<CustomerResponse>> SearchCustomersByNameAsync(string name);

    }
}