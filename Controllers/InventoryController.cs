namespace dotnet_backend.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using dotnet_backend.Services;

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
        public async Task<IActionResult> Index()
        {
            var items = await _inventoryService.GetInventoryListAsync();
            return View(items); 
        }
        
    }
}