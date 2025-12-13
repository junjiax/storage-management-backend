using dotnet_backend.DTOs.Common;
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

        // 🔹 Lấy danh sách tất cả khách hàng
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<CustomerResponse>>>> GetCustomers()
        {
            try
            {
                var customers = await _customerService.GetCustomerListAsync();
                return Ok(ApiResponse<List<CustomerResponse>>.Ok(
                    data: customers,
                    message: "Customers retrieved successfully"
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<CustomerResponse>>.Fail(
                    message: $"Internal server error: {ex.Message}",
                    statusCode: 500
                ));
            }
        }

        // 🔹 Lấy chi tiết khách hàng theo ID
        [HttpGet("{customerId}")]
        public async Task<ActionResult<ApiResponse<CustomerResponse>>> GetCustomerById(int customerId)
        {
            var customer = await _customerService.GetCustomerByIdAsync(customerId);
            if (customer == null)
            {
                return NotFound(ApiResponse<CustomerResponse>.Fail(
                    message: $"Customer with ID {customerId} not found",
                    statusCode: 404
                ));
            }

            return Ok(ApiResponse<CustomerResponse>.Ok(
                data: customer,
                message: "Customer retrieved successfully"
            ));
        }

        // 🔹 Thêm khách hàng mới
        [HttpPost]
        public async Task<ActionResult<ApiResponse<CustomerResponse>>> AddCustomer([FromBody] CreateCustomerRequest request)
        {
            try
            {
                var customer = await _customerService.AddCustomerItemAsync(request);
                return CreatedAtAction(
                    nameof(GetCustomerById),
                    new { customerId = customer.CustomerId },
                    ApiResponse<CustomerResponse>.Ok(
                        data: customer,
                        message: "Customer created successfully"
                    )
                );
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<CustomerResponse>.Fail(
                    message: $"Failed to create customer: {ex.Message}",
                    statusCode: 400
                ));
            }
        }

        // 🔹 Cập nhật thông tin khách hàng
        [HttpPut("{customerId}")]
        public async Task<ActionResult<ApiResponse<CustomerResponse>>> UpdateCustomer(int customerId, [FromBody] UpdateCustomerRequest request)
        {
            var updatedCustomer = await _customerService.UpdateCustomerItemAsync(customerId, request);

            if (updatedCustomer == null)
            {
                return NotFound(ApiResponse<CustomerResponse>.Fail(
                    message: $"Customer with ID {customerId} not found",
                    statusCode: 404
                ));
            }

            return Ok(ApiResponse<CustomerResponse>.Ok(
                data: updatedCustomer,
                message: "Customer updated successfully"
            ));
        }

        // 🔹 Xóa khách hàng
        [HttpDelete("{customerId}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteCustomer(int customerId)
        {
            var success = await _customerService.DeleteCustomerItemAsync(customerId);
            if (!success)
            {
                return NotFound(ApiResponse<bool>.Fail(
                    message: $"Customer with ID {customerId} not found",
                    statusCode: 404
                ));
            }

            return Ok(ApiResponse<bool>.Ok(
                data: true,
                message: "Customer deleted successfully"
            ));
        }
    }
}
