using Microsoft.EntityFrameworkCore;
using dotnet_backend.Data;
using dotnet_backend.DTOs.Customer;
using dotnet_backend.Models;


namespace dotnet_backend.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly StoreDbContext _context;

        public CustomerService(StoreDbContext context)
        {
            _context = context;
        }

        // Lấy danh sách tất cả khách hàng
        public async Task<List<CustomerResponse>> GetCustomerListAsync()
        {
            return await _context.Customers
                .Select(c => new CustomerResponse
                {
                    CustomerId = c.CustomerId,
                    Name = c.Name,
                    Phone = c.Phone,
                    Email = c.Email,
                    Address = c.Address,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();
        }

        // Lấy khách hàng theo Id
        public async Task<CustomerResponse?> GetCustomerByIdAsync(int customerId)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (customer == null) return null;

            return new CustomerResponse
            {
                CustomerId = customer.CustomerId,
                Name = customer.Name,
                Phone = customer.Phone,
                Email = customer.Email,
                Address = customer.Address,
                CreatedAt = customer.CreatedAt
            };
        }

        // Thêm khách hàng mới
        public async Task<CustomerResponse> AddCustomerItemAsync(CreateCustomerRequest request)
        {
            var customer = new Customer
            {
                Name = request.Name,
                Phone = request.Phone,
                Email = request.Email,
                Address = request.Address,
                CreatedAt = DateTime.UtcNow
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return new CustomerResponse
            {
                CustomerId = customer.CustomerId,
                Name = customer.Name,
                Phone = customer.Phone,
                Email = customer.Email,
                Address = customer.Address,
                CreatedAt = customer.CreatedAt
            };
        }

        // Cập nhật thông tin khách hàng
        public async Task<CustomerResponse?> UpdateCustomerItemAsync(int customerId, UpdateCustomerRequest request)
        {
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null) return null;

            if (!string.IsNullOrEmpty(request.Name)) customer.Name = request.Name;
            if (request.Phone != null) customer.Phone = request.Phone;
            if (request.Email != null) customer.Email = request.Email;
            if (request.Address != null) customer.Address = request.Address;

            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();

            return new CustomerResponse
            {
                CustomerId = customer.CustomerId,
                Name = customer.Name,
                Phone = customer.Phone,
                Email = customer.Email,
                Address = customer.Address,
                CreatedAt = customer.CreatedAt
            };
        }

        // Xóa khách hàng
        public async Task<bool> DeleteCustomerItemAsync(int customerId)
        {
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null) return false;

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
            return true;
        }

        // Kiểm tra khách hàng tồn tại
        public async Task<bool> CustomerExistsAsync(int customerId)
        {
            return await _context.Customers.AnyAsync(c => c.CustomerId == customerId);
        }
        
        public async Task<List<CustomerResponse>> SearchCustomersByNameAsync(string name)
        {
            return await _context.Customers
                .Where(c => c.Name.Contains(name))
                .Select(c => new CustomerResponse
                {
                    CustomerId = c.CustomerId,
                    Name = c.Name,
                    Phone = c.Phone,
                    Email = c.Email,
                    Address = c.Address,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();
        }

    }
}