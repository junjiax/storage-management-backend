using dotnet_backend.Data;
using dotnet_backend.DTOs.Report;
using dotnet_backend.Models;
using dotnet_backend.Repositories;
using dotnet_backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace dotnet_backend.Services.Implementations
{
   public class ReportService : IReportService
   {

      private readonly IReportRepository _reportRepository;
      public ReportService(IReportRepository reportRepository)
      {
         _reportRepository = reportRepository;
      }
      public async Task<RevenueReportResponse> GetFlexibleRevenueReportAsync(RevenueReportRequest request)
      {
         if (request.StartDate.HasValue && request.EndDate.HasValue && request.StartDate > request.EndDate)
         {
            throw new ArgumentException("Ngày bắt đầu không thể sau ngày kết thúc.");
         }

         return await _reportRepository.GetFlexibleRevenueReportAsync(request);
      }
      public async Task<InventoryReportReponse> GetLowStockReportAsync(InventoryReportRequest request)
      {
         return await _reportRepository.GetLowStockReportAsync(request);
      }

      public async Task<PromotionReportResponse> GetPromotionSimpleReportAsync(PromotionReportRequest request)
      {
         var now = DateTime.UtcNow;

         if (request.Year > now.Year)
         {
            throw new ArgumentException($"Năm {request.Year} nằm ở tương lai. Không thể xem báo cáo.");
         }

         if (request.Year == now.Year && request.Month > now.Month)
         {
            throw new ArgumentException($"Tháng {request.Month}/{request.Year} nằm ở tương lai. Không thể xem báo cáo.");
         }

          if (request.Year < 2020 /* vd: 2020 là năm thành lập */)
         {
            throw new ArgumentException("Chỉ hỗ trợ báo cáo từ năm 2020 trở đi.");
         }

         return await _reportRepository.GetPromotionSimpleReportAsync(request);
      }

      public async Task<SimpleReportResponse> GetSimpleReportAsync(SimpleReportRequest request)
      {
         // Xử lý logic nghiệp vụ/validation
         if (request.StartDate.HasValue && request.EndDate.HasValue && request.StartDate > request.EndDate)
         {
            throw new ArgumentException("Ngày bắt đầu không thể sau ngày kết thúc.");
         }

         // Gọi Repository
         return await _reportRepository.GetSimpleReportAsync(request);
      }

      public async Task<RevenueByMothResponse> GetRevenueByMonthAsync(RevenueByMothRequest request)
      {
         // --- Xử lý Validation ---
         int currentYear = DateTime.UtcNow.Year;
         if (request.Year > currentYear)
         {
            throw new ArgumentException($"Năm {request.Year} nằm ở tương lai. Không thể xem báo cáo.");
         }
         if (request.Year < 2020) // Ví dụ: Giới hạn năm cũ nhất
         {
            throw new ArgumentException("Chỉ hỗ trợ báo cáo từ năm 2020 trở đi.");
         }

         // --- Gọi Repository ---
         return await _reportRepository.GetRevenueByMonthAsync(request);
      }
   }
}
