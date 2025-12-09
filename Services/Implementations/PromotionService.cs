using Microsoft.EntityFrameworkCore;
using dotnet_backend.Data;
using dotnet_backend.DTOs.Promotion;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dotnet_backend.Models;


namespace dotnet_backend.Services
{
    public class PromotionService : IPromotionService
    {
        private readonly StoreDbContext _context;

        public PromotionService(StoreDbContext context)
        {
            _context = context;
        }

        // Lấy danh sách tất cả khuyến mãi
        public async Task<List<PromotionResponse>> GetPromotionListAsync()
        {
            var promotions = await _context.Promotions.ToListAsync();
            var now = DateTime.UtcNow;

            // Cập nhật trạng thái theo logic
            foreach (var p in promotions)
            {
                bool shouldBeInactive =
                    p.StartDate > now || // chưa bắt đầu
                    p.EndDate < now ||   // đã kết thúc
                    (p.UsageLimit > 0 && p.UsedCount >= p.UsageLimit); // hết lượt

                // Nếu trạng thái thay đổi thì update luôn DB
                if (shouldBeInactive && p.Status != "inactive")
                {

                    p.Status = "inactive";
                    _context.Promotions.Update(p);
                }
            }

            await _context.SaveChangesAsync();

            return promotions.Select(p => new PromotionResponse
            {
                PromoId = p.PromoId,
                PromoCode = p.PromoCode,
                Description = p.Description,
                DiscountType = p.DiscountType,
                DiscountValue = p.DiscountValue,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                MinOrderAmount = p.MinOrderAmount,
                UsageLimit = p.UsageLimit,
                UsedCount = p.UsedCount,
                Status = p.Status
            }).ToList();
        }

        public async Task<List<PromotionResponse>?> GetPromotionsWithMinOrderAmountGreaterThanAsync(decimal minOrderAmount)
        {
            var now = DateTime.UtcNow;

            var promotions = await _context.Promotions
                .Where(p => p.MinOrderAmount <= minOrderAmount
                            && p.EndDate >= now
                            && p.Status == "active")
                .ToListAsync();

            var promotionResponses = promotions.Select(p => new PromotionResponse
            {
                PromoId = p.PromoId,
                PromoCode = p.PromoCode,
                Description = p.Description,
                DiscountType = p.DiscountType,
                DiscountValue = p.DiscountValue,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                MinOrderAmount = p.MinOrderAmount,
                UsageLimit = p.UsageLimit,
                UsedCount = p.UsedCount,
                Status = p.Status
            }).ToList();

            return promotionResponses;
        }

        // Lấy chi tiết khuyến mãi theo ID
        public async Task<PromotionResponse?> GetPromotionByIdAsync(int id)
        {
            var p = await _context.Promotions.FindAsync(id);
            if (p == null) return null;

            return new PromotionResponse
            {
                PromoId = p.PromoId,
                PromoCode = p.PromoCode,
                Description = p.Description,
                DiscountType = p.DiscountType,
                DiscountValue = p.DiscountValue,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                MinOrderAmount = p.MinOrderAmount,
                UsageLimit = p.UsageLimit,
                UsedCount = p.UsedCount,
                Status = p.Status
            };
        }

        // Thêm khuyến mãi mới
        public async Task<PromotionResponse> AddPromotionAsync(CreatePromotionRequest request)
        {
            var promotion = new Promotion
            {
                PromoCode = request.PromoCode,
                Description = request.Description,
                DiscountType = request.DiscountType,
                DiscountValue = request.DiscountValue,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                MinOrderAmount = request.MinOrderAmount,
                UsageLimit = request.UsageLimit,
                UsedCount = 0,
                //Status = request.Status
            };

            _context.Promotions.Add(promotion);
            await _context.SaveChangesAsync();

            return new PromotionResponse
            {
                PromoId = promotion.PromoId,
                PromoCode = promotion.PromoCode,
                Description = promotion.Description,
                DiscountType = promotion.DiscountType,
                DiscountValue = promotion.DiscountValue,
                StartDate = promotion.StartDate,
                EndDate = promotion.EndDate,
                MinOrderAmount = promotion.MinOrderAmount,
                UsageLimit = promotion.UsageLimit,
                UsedCount = promotion.UsedCount,
                Status = promotion.Status
            };
        }

        // Cập nhật khuyến mãi
        public async Task<PromotionResponse> UpdatePromotionAsync(int id, UpdatePromotionRequest request)
        {
            var promotion = await _context.Promotions.FindAsync(id);
            if (promotion == null) return null!; // Hoặc ném NotFound exception nếu muốn

            promotion.Description = request.Description ?? promotion.Description;
            //promotion.DiscountType = request.DiscountType ?? promotion.DiscountType;
            promotion.DiscountValue = request.DiscountValue ?? promotion.DiscountValue;
            //promotion.StartDate = request.StartDate ?? promotion.StartDate;
            promotion.EndDate = request.EndDate ?? promotion.EndDate;
            promotion.MinOrderAmount = request.MinOrderAmount ?? promotion.MinOrderAmount;
            promotion.UsageLimit = request.UsageLimit ?? promotion.UsageLimit;
            promotion.Status = request.Status ?? promotion.Status;

            _context.Promotions.Update(promotion);
            await _context.SaveChangesAsync();

            return new PromotionResponse
            {
                PromoId = promotion.PromoId,
                PromoCode = promotion.PromoCode,
                Description = promotion.Description,
                DiscountType = promotion.DiscountType,
                DiscountValue = promotion.DiscountValue,
                StartDate = promotion.StartDate,
                EndDate = promotion.EndDate,
                MinOrderAmount = promotion.MinOrderAmount,
                UsageLimit = promotion.UsageLimit,
                UsedCount = promotion.UsedCount,
                Status = promotion.Status
            };
        }

        // Xóa khuyến mãi
        public async Task<bool> DeletePromotionAsync(int id)
        {
            var promotion = await _context.Promotions.FindAsync(id);
            if (promotion == null) return false;

            _context.Promotions.Remove(promotion);
            await _context.SaveChangesAsync();
            return true;
        }

        // Kiểm tra tồn tại
        public async Task<bool> PromotionExistAsync(int id)
        {
            return await _context.Promotions.AnyAsync(p => p.PromoId == id);
        }
    }
}