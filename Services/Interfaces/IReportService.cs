using dotnet_backend.DTOs.Report;

namespace dotnet_backend.Services.Interfaces
{
   public interface IReportService
   {
      Task<RevenueReportResponse> GetFlexibleRevenueReportAsync(RevenueReportRequest request);

      Task<InventoryReportReponse> GetLowStockReportAsync(InventoryReportRequest request);

      Task<PromotionReportResponse> GetPromotionSimpleReportAsync(PromotionReportRequest request);

      Task<SimpleReportResponse> GetSimpleReportAsync(SimpleReportRequest request);

      Task<RevenueByMothResponse> GetRevenueByMonthAsync(RevenueByMothRequest request);

   }
}
