using System.ComponentModel.DataAnnotations;

namespace dotnet_backend.DTOs.Report
{
   public class PromotionReportRequest
   {
      [Required]
      public int Year { get; set; }

      [Required]
      [Range(1, 12)] // Đảm bảo tháng là từ 1-12
      public int Month { get; set; }
   }

   public class PromotionReportResponse
   {
      public int TotalPromotionsActiveInMonth { get; set; }
      public int TotalPromotionsUsed { get; set; }
   }

   
}
