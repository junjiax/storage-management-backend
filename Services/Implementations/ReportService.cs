using System.Text.RegularExpressions;
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
         ValidateMonthRange(request.StartDate, request.EndDate);
         // Gọi Repository
         return await _reportRepository.GetSimpleReportAsync(request);
      }

      public async Task<RatioByCategoryResponse> GetRatioByCategoryAsync(RatioByCategoryRequest request)
      {
         // --- Validate ---
         ValidateMonthRange(request.StartDate, request.EndDate);

         // --- Gọi Repository ---
         return await _reportRepository.GetRatioByCategoryAsync(request);
      }
      public async Task<RevenueByMothResponse> GetRevenueByMonthAsync(RevenueByMothRequest request)
      {
         // --- Validate ---
         ValidateMonthRange(request.StartDate, request.EndDate);

         // --- Gọi Repository ---
         return await _reportRepository.GetRevenueByMonthAsync(request);
      }
      private (int Year, int Month) ParseYearMonth(string ym)
      {
         if (string.IsNullOrWhiteSpace(ym))
            throw new ArgumentException("Tháng không được để trống.");

         // Format phải đúng dạng YYYY-MM
         if (!Regex.IsMatch(ym, @"^\d{4}-(0[1-9]|1[0-2])$"))
            throw new ArgumentException("Định dạng tháng không hợp lệ. Phải là YYYY-MM (ví dụ: 2025-01).");

         var parts = ym.Split('-');
         int year = int.Parse(parts[0]);
         int month = int.Parse(parts[1]);

         // Validate tháng hợp lệ 1–12
         if (month < 1 || month > 12)
            throw new ArgumentException("Tháng phải nằm trong khoảng 1 đến 12.");

         return (year, month);
      }

      private void ValidateMonthRange(string startYm, string endYm)
      {
         var (startYear, startMonth) = ParseYearMonth(startYm);
         var (endYear, endMonth) = ParseYearMonth(endYm);

         var startDate = new DateTime(startYear, startMonth, 1);
         var endDate = new DateTime(endYear, endMonth, 1);

         // --- Validate Start <= End ---
         if (startDate > endDate)
            throw new ArgumentException("Tháng bắt đầu không thể sau tháng kết thúc.");

         // --- Validate range không vượt quá 12 tháng ---
         int totalMonths = ((endYear - startYear) * 12) + (endMonth - startMonth) + 1;
         if (totalMonths > 12)
            throw new ArgumentException("Khoảng thời gian không được vượt quá 12 tháng.");

         // Không throw → valid
      }


      public async Task<OrdersByDayResponse> GetOrdersByDayAsync(OrdersByDayRequest request)
      {
         // --- Xử lý Validation ---
         if (request.Year > DateTime.Now.Year)
         {
            throw new ArgumentException("Năm không được lớn hơn năm hiện tại.");
         }

         // Ví dụ: Nếu là năm hiện tại, tháng không được lớn hơn tháng hiện tại
         if (request.Year == DateTime.Now.Year && request.Month > DateTime.Now.Month)
         {
            throw new ArgumentException("Không thể lấy báo cáo cho tháng trong tương lai.");
         }
         // --- Gọi Repository ---
         return await _reportRepository.GetOrdersByDayAsync(request);
      }
   }
}
