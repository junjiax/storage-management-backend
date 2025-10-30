namespace dotnet_backend.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using dotnet_backend.Services;

    [ApiController]
    [Route("[controller]")]
    public class HomeController : ControllerBase
    {
        private readonly IStorageService _storageService;

        public HomeController(IStorageService storageService)
        {
            _storageService = storageService;
        }

        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            var status = _storageService.GetStatus();
            return Ok(new { Status = status });
        }
    }
}