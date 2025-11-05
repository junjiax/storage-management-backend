using dotnet_backend.DTOs.Promotion;
using dotnet_backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace dotnet_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PromotionController : ControllerBase
    {
        private readonly IPromotionService _promotionService;

        public PromotionController(IPromotionService promotionService)
        {
            _promotionService = promotionService;
        }

        // Lấy danh sách tất cả khuyến mãi
        [HttpGet]
        public async Task<ActionResult<List<PromotionResponse>>> GetPromotions()
        {
            var promotions = await _promotionService.GetPromotionListAsync();
            return Ok(promotions);
        }

        // Lấy chi tiết khuyến mãi theo ID
        [HttpGet("{promotionId}")]
        public async Task<ActionResult<PromotionResponse>> GetPromotionById(int promotionId)
        {
            var promotion = await _promotionService.GetPromotionByIdAsync(promotionId);
            if (promotion == null)
            {
                return NotFound();
            }
            return Ok(promotion);
        }

        // Thêm khuyến mãi mới
        [HttpPost]
        public async Task<ActionResult<PromotionResponse>> AddPromotion([FromBody] CreatePromotionRequest request)
        {
            var promotion = await _promotionService.AddPromotionAsync(request);
            return CreatedAtAction(nameof(GetPromotionById), new { promotionId = promotion.PromoId }, promotion);
        }

        // Cập nhật khuyến mãi
        [HttpPut("{promotionId}")]
        public async Task<ActionResult<PromotionResponse>> UpdatePromotion(int promotionId, [FromBody] UpdatePromotionRequest request)
        {
            var updatedPromotion = await _promotionService.UpdatePromotionAsync(promotionId, request);
            if (updatedPromotion == null)
            {
                return NotFound();
            }
            return Ok(updatedPromotion);
        }

        // Xóa khuyến mãi
        [HttpDelete("{promotionId}")]
        public async Task<IActionResult> DeletePromotion(int promotionId)
        {
            var success = await _promotionService.DeletePromotionAsync(promotionId);
            if (!success)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}
