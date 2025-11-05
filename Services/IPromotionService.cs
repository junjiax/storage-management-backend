using dotnet_backend.DTOs.Promotion;
using System.Collections.Generic;
using System.Threading.Tasks;
using dotnet_backend.Models;

namespace dotnet_backend.Services
{
    public interface IPromotionService
    {
        Task<List<PromotionResponse>> GetPromotionListAsync();
        Task<PromotionResponse?> GetPromotionByIdAsync(int id);
        Task<PromotionResponse> AddPromotionAsync(CreatePromotionRequest request);
        Task<PromotionResponse> UpdatePromotionAsync(int id, UpdatePromotionRequest request);

        Task<bool> DeletePromotionAsync(int id);
        Task<bool> PromotionExistAsync(int id);
    }
}
