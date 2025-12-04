using dotnet_backend.DTOs.Common;
using dotnet_backend.DTOs.Report;
using dotnet_backend.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace dotnet_backend.Controllers
{
   [Route("api/[controller]")]
   [ApiController]
   public class ReportController : ControllerBase
   {
      private readonly IReportService _reportService;
      public ReportController(IReportService reportService)
      {
         _reportService = reportService;
      }

      [HttpGet("flexible-revenue")]
      // Cập nhật kiểu trả về để Swagger/OpenAPI hiểu rõ hơn
      [ProducesResponseType(typeof(ApiResponse<RevenueReportResponse>), 200)]
      [ProducesResponseType(typeof(ApiResponse), 400)]
      [ProducesResponseType(typeof(ApiResponse), 500)]
      public async Task<IActionResult> GetFlexibleRevenueReport([FromQuery] RevenueReportRequest request)
      {
         try
         {
            // 1. Gọi Service
            var reportData = await _reportService.GetFlexibleRevenueReportAsync(request);

            // 2. Gói kết quả thành công bằng ApiResponse<T>
            var response = ApiResponse<RevenueReportResponse>.Ok(reportData, "Lấy báo cáo thành công.");

            return Ok(response); // Trả về 200 OK + object response
         }
         catch (ArgumentException ex) // Lỗi do người dùng (validation)
         {
            // 3. Gói lỗi Bad Request bằng ApiResponse
            var response = ApiResponse.Fail(ex.Message, 400);

            return BadRequest(response); // Trả về 400 Bad Request + object response
         }
         catch (Exception ex) // Lỗi hệ thống
         {
            // (Nên log lỗi `ex` ở đây)

            // 4. Gói lỗi Server Error bằng ApiResponse
            var response = ApiResponse.Fail("Đã xảy ra lỗi hệ thống, vui lòng thử lại.", 500);

            return StatusCode(500, response); // Trả về 500 + object response
         }
      }


      [HttpGet("inventory-report")]
      [ProducesResponseType(typeof(ApiResponse<InventoryReportReponse>), 200)]
      [ProducesResponseType(typeof(ApiResponse), 500)]
      public async Task<IActionResult> GetLowStockReport([FromQuery] InventoryReportRequest request)
      {
         try
         {
            var reportData = await _reportService.GetLowStockReportAsync(request); // Trả về InventoryReportResponse

            var response = ApiResponse<InventoryReportReponse>.Ok(reportData, "Lấy báo cáo hàng sắp hết thành công.");

            return Ok(response);
         }
         catch (Exception ex)
         {
            var response = ApiResponse.Fail("Đã xảy ra lỗi hệ thống, vui lòng thử lại.", 500);
            return StatusCode(500, response);
         }
      }



      [HttpGet("promotion-report")] // <-- Endpoint bạn yêu cầu
      [ProducesResponseType(typeof(ApiResponse<PromotionReportResponse>), 200)]
      [ProducesResponseType(typeof(ApiResponse), 400)]
      [ProducesResponseType(typeof(ApiResponse), 500)]
      public async Task<IActionResult> GetPromotionReport([FromQuery] PromotionReportRequest request)
      {
         try
         {
            // (ModelState.IsValid sẽ tự động kiểm tra [Range(1,12)] cho Month)
            if (!ModelState.IsValid)
            {
               return BadRequest(ApiResponse.Fail("Dữ liệu đầu vào không hợp lệ. 'year' và 'month' (1-12) là bắt buộc.", 400));
            }

            var reportData = await _reportService.GetPromotionSimpleReportAsync(request);

            var response = ApiResponse<PromotionReportResponse>.Ok(reportData, "Lấy báo cáo khuyến mãi thành công.");

            return Ok(response);
         }
         catch (Exception ex)
         {
            var response = ApiResponse.Fail("Đã xảy ra lỗi hệ thống, vui lòng thử lại.", 500);
            return StatusCode(500, response);
         }
      }


      [HttpGet("simple-report")] // API mới
      [ProducesResponseType(typeof(ApiResponse<SimpleReportResponse>), 200)]
      [ProducesResponseType(typeof(ApiResponse), 400)]
      [ProducesResponseType(typeof(ApiResponse), 500)]
      public async Task<IActionResult> GetSimpleReport([FromQuery] SimpleReportRequest request)
      {
         try
         {
            var reportData = await _reportService.GetSimpleReportAsync(request);

            var response = ApiResponse<SimpleReportResponse>.Ok(reportData, "Lấy báo cáo tổng hợp thành công.");

            return Ok(response);
         }
         catch (ArgumentException ex) // Bắt lỗi validation từ Service
         {
            return BadRequest(ApiResponse.Fail(ex.Message, 400));
         }
         catch (Exception ex)
         {
            // (Nên log lỗi 'ex' ở đây)
            var response = ApiResponse.Fail("Đã xảy ra lỗi hệ thống, vui lòng thử lại.", 500);
            return StatusCode(500, response);
         }
      }

      [HttpGet("revenue-by-month")] // API: GET /api/reports/revenue-by-month?Year=2025
      [ProducesResponseType(typeof(ApiResponse<RevenueByMothResponse>), 200)]
      [ProducesResponseType(typeof(ApiResponse), 400)]
      [ProducesResponseType(typeof(ApiResponse), 500)]
      public async Task<IActionResult> GetRevenueByMonth([FromQuery] RevenueByMothRequest request)
      {
         try
         {
            var reportData = await _reportService.GetRevenueByMonthAsync(request);

            var response = ApiResponse<RevenueByMothResponse>.Ok(reportData, "Lấy báo cáo doanh thu theo tháng thành công.");

            return Ok(response);
         }
         catch (ArgumentException ex) // Bắt lỗi validation từ Service
         {
            return BadRequest(ApiResponse.Fail(ex.Message, 400));
         }
         catch (Exception ex)
         {
            // (Nên log lỗi 'ex' ở đây)
            var response = ApiResponse.Fail("Đã xảy ra lỗi hệ thống, vui lòng thử lại.", 500);
            return StatusCode(500, response);
         }
      }
   }
}
