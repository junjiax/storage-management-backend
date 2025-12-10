using Microsoft.AspNetCore.Mvc;
using dotnet_backend.Services;
using dotnet_backend.DTOs.Common;
using dotnet_backend.DTOs.Inventory;

namespace dotnet_backend.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class InventoryController : Controller
    {
        private readonly IInventoryService _inventoryService;

        public InventoryController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<InventoryResponse>>>> Index()
        {
            var items = await _inventoryService.GetInventoryListAsync();
            return Ok(ApiResponse<List<InventoryResponse>>.Ok(items,"Retrieved inventories succesfully.", 200));
        }

        [HttpPost]
        public async Task<IActionResult> AddInventory([FromBody] InventoryRequest request)
        {
            var items = await _inventoryService.AddInventoryItemAsync(request);
            return Ok(ApiResponse<InventoryResponse>.Ok(items, "Added inventory", 201));
        }

        [HttpPut("{inventoryId}")]
        public async Task<IActionResult> UpdateInventory(int inventoryId, [FromBody] InventoryRequest request)
        {
            var updatedItem = await _inventoryService.UpdateInventoryItemAsync(inventoryId, request);
            if (updatedItem == null)
            {
                return NotFound(ApiResponse<InventoryResponse>.Fail("Inventory item not found", 404));
            }

            return Ok(ApiResponse<InventoryResponse>.Ok(updatedItem));
        }

        [HttpDelete("{inventoryId}")]
        public async Task<IActionResult> DeleteInventory(int inventoryId)
        {
            var success = await _inventoryService.DeleteInventoryItemAsync(inventoryId);
            if (!success)
            {
                return NotFound(ApiResponse<bool>.Fail("Inventory item not found", 404));
            }

            return Ok(ApiResponse<bool>.Ok("Inventory is deleted"));
        }

        [HttpGet("{inventoryId}")]
        public async Task<IActionResult> GetInventoryById(int inventoryId)
        {
            var item = await _inventoryService.GetInventoryItemByIdAsync(inventoryId);
            if (item == null)
            {
                return NotFound(ApiResponse<InventoryResponse>.Fail("Inventory item not found", 404));
            }

            return Ok(ApiResponse<InventoryResponse>.Ok(item));
        }

        [HttpGet("product-log/{productId}")]
        public async Task<IActionResult> GetProductLog(int productId)
        {
            var item = await _inventoryService.GetProductLogAsync(productId);
            if (item == null)
            {
                return NotFound(ApiResponse<InventoryResponse>.Fail("Inventory item not found", 404));
            }

            return Ok(ApiResponse<List<InventoryLogDto>>.Ok(item));
        }

        [HttpGet("exists/{inventoryId}")]
        public async Task<IActionResult> InventoryItemExists(int inventoryId)
        {
            var exists = await _inventoryService.InventoryItemExistsAsync(inventoryId);
            return Ok(ApiResponse<bool>.Ok(exists));
        }
    }
}