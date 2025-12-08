using dotnet_backend.DTOs.Report;

namespace dotnet_backend.Repositories
{
   public interface IReportRepository
   {
      Task<RevenueReportResponse> GetFlexibleRevenueReportAsync(RevenueReportRequest request);

      Task<InventoryReportReponse> GetLowStockReportAsync(InventoryReportRequest request);

      Task<PromotionReportResponse> GetPromotionSimpleReportAsync(PromotionReportRequest request);
   }
}
