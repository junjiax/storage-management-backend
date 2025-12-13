using System.ComponentModel.DataAnnotations;

namespace dotnet_backend.DTOs.Report
{
   public class OrdersByDayRequest
   {
      [Required]
      [Range(1, 12)] // Đảm bảo tháng là từ 1-12
      public int Month { get; set; }

      [Required]
      public int Year { get; set; }
   }
   public class DailyOrdersData
   {
      public int Orders { get; set; }
      public DateTime Date { get; set; }
   }
   public class OrdersByDayResponse
   {
      public int Month { get; set; }
      public int Year { get; set; }
      public List<DailyOrdersData>? DailyOrders { get; set; }
   }
}
