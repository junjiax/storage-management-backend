using dotnet_backend.DTOs.Common;
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

        // 🔹 Lấy danh sách tất cả khuyến mãi
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<PromotionResponse>>>> GetPromotions()
        {
            var promotions = await _promotionService.GetPromotionListAsync();
            return Ok(ApiResponse<List<PromotionResponse>>.Ok(
                data: promotions,
                message: "Promotions retrieved successfully"
            ));
        }

        // 🔹 Lấy chi tiết khuyến mãi theo ID
        [HttpGet("{promotionId}")]
        public async Task<ActionResult<ApiResponse<PromotionResponse>>> GetPromotionById(int promotionId)
        {
            var promotion = await _promotionService.GetPromotionByIdAsync(promotionId);
            if (promotion == null)
            {
                return NotFound(ApiResponse<PromotionResponse>.Fail(
                    message: $"Promotion with ID {promotionId} not found",
                    statusCode: 404
                ));
            }

            return Ok(ApiResponse<PromotionResponse>.Ok(
                data: promotion,
                message: "Promotion retrieved successfully"
            ));
        }

        // 🔹 Thêm khuyến mãi mới
        [HttpPost]
        public async Task<ActionResult<ApiResponse<PromotionResponse>>> AddPromotion([FromBody] CreatePromotionRequest request)
        {
            var promotion = await _promotionService.AddPromotionAsync(request);

            return CreatedAtAction(
                nameof(GetPromotionById),
                new { promotionId = promotion.PromoId },
                ApiResponse<PromotionResponse>.Ok(
                    data: promotion,
                    message: "Promotion created successfully"
                )
            );
        }

        // 🔹 Cập nhật khuyến mãi
        [HttpPut("{promotionId}")]
        public async Task<ActionResult<ApiResponse<PromotionResponse>>> UpdatePromotion(int promotionId, [FromBody] UpdatePromotionRequest request)
        {
            var updatedPromotion = await _promotionService.UpdatePromotionAsync(promotionId, request);

            if (updatedPromotion == null)
            {
                return NotFound(ApiResponse<PromotionResponse>.Fail(
                    message: $"Promotion with ID {promotionId} not found",
                    statusCode: 404
                ));
            }

            return Ok(ApiResponse<PromotionResponse>.Ok(
                data: updatedPromotion,
                message: "Promotion updated successfully"
            ));
        }

        // 🔹 Xóa khuyến mãi
        [HttpDelete("{promotionId}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeletePromotion(int promotionId)
        {
            var success = await _promotionService.DeletePromotionAsync(promotionId);

            if (!success)
            {
                return NotFound(ApiResponse<PromotionResponse>.Fail(
                    message: $"Promotion with ID {promotionId} not found",
                    statusCode: 404
                ));
            }

            return Ok(ApiResponse<bool>.Ok(
                data: true,
                message: "Promotion deleted successfully"
            ));
        }
    }
}
