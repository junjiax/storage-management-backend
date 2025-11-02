using dotnet_backend.DTOs.Supplier;
using dotnet_backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace dotnet_backend.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class SupplierController : ControllerBase
    {
        private readonly ISupplierService _supplierService;

        public SupplierController(ISupplierService supplierService)
        {
            _supplierService = supplierService;
        }

        [HttpGet]
        public async Task<ActionResult<List<SupplierResponse>>> GetSuppliers()
        {
            var suppliers = await _supplierService.GetSupplierListAsync();
            return Ok(suppliers);
        }

        [HttpGet("{supplierId}")]
        public async Task<ActionResult<SupplierResponse>> GetSupplierById(int supplierId)
        {
            var supplier = await _supplierService.GetSupplierByIdAsync(supplierId);
            if (supplier == null)
            {
                return NotFound();
            }
            return Ok(supplier);
        }

        [HttpPost]
        public async Task<ActionResult<SupplierResponse>> AddSupplier([FromBody] SupplierRequest request)
        {
            var supplier = await _supplierService.AddSupplierItemAsync(request);
            return CreatedAtAction(nameof(GetSupplierById), new { supplierId = supplier.SupplierId }, supplier);
        }

        [HttpPut("{supplierId}")]
        public async Task<ActionResult<SupplierResponse>> UpdateSupplier(int supplierId, [FromBody] SupplierRequest request)
        {
            var updatedSupplier = await _supplierService.UpdateSupplierItemAsync(supplierId, request);
            if (updatedSupplier == null)
            {
                return NotFound();
            }
            return Ok(updatedSupplier);
        }

        [HttpDelete("{supplierId}")]
        public async Task<IActionResult> DeleteSupplier(int supplierId)
        {
            var success = await _supplierService.DeleteSupplierItemAsync(supplierId);
            if (!success)
            {
                return NotFound();
            }
            return NoContent();
        }

        [HttpGet("exists/{supplierId}")]
        public async Task<IActionResult> SupplierExists(int supplierId)
        {
            var exists = await _supplierService.SupplierExistsAsync(supplierId);
            return Ok(exists);
        }
    }
}