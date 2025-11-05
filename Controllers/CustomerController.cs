using dotnet_backend.DTOs.Customer;
using dotnet_backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace dotnet_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        // Lấy danh sách tất cả khách hàng
        [HttpGet]
        public async Task<ActionResult<List<CustomerResponse>>> GetCustomers()
        {
            var customers = await _customerService.GetCustomerListAsync();
            return Ok(customers);
        }

        // Lấy chi tiết khách hàng theo ID
        [HttpGet("{customerId}")]
        public async Task<ActionResult<CustomerResponse>> GetCustomerById(int customerId)
        {
            var customer = await _customerService.GetCustomerByIdAsync(customerId);
            if (customer == null)
            {
                return NotFound();
            }
            return Ok(customer);
        }

        // Thêm khách hàng mới
        [HttpPost]
        public async Task<ActionResult<CustomerResponse>> AddCustomer([FromBody] CreateCustomerRequest request)
        {
            var customer = await _customerService.AddCustomerItemAsync(request);
            return CreatedAtAction(
                nameof(GetCustomerById),
                new { customerId = customer.CustomerId },
                customer
            );
        }

        // Cập nhật thông tin khách hàng
        [HttpPut("{customerId}")]
        public async Task<ActionResult<CustomerResponse>> UpdateCustomer(int customerId, [FromBody] UpdateCustomerRequest request)
        {
            var updatedCustomer = await _customerService.UpdateCustomerItemAsync(customerId, request);
            if (updatedCustomer == null)
            {
                return NotFound();
            }
            return Ok(updatedCustomer);
        }

        // Xóa khách hàng
        [HttpDelete("{customerId}")]
        public async Task<IActionResult> DeleteCustomer(int customerId)
        {
            var success = await _customerService.DeleteCustomerItemAsync(customerId);
            if (!success)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}
