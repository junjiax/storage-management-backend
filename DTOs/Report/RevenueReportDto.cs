using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using dotnet_backend.DTOs.Order;

namespace dotnet_backend.DTOs.Report
{
   public class RevenueReportRequest
   {
      public DateTime? StartDate { get; set; }
      public DateTime? EndDate { get; set; }
      public int? ProductId { get; set; }
      public int? EmployeeId { get; set; }
      public int PageNumber { get; set; } = 1;
      public int PageSize { get; set; } = 20;
   }
   public class PaymentReportResponse
   {
      public int PaymentId { get; set; }
      public int OrderId { get; set; }
      public decimal Amount { get; set; }
      public DateTime PaymentDate { get; set; }
      public string? PaymentMethod { get; set; }

   }


   public class RevenueReportResponse
   {
      public decimal TotalRevenue { get; set; }
      public PaginatedResult<PaymentReportResponse>? Payments { get; set; }
   }


   public class PaginatedResult<T>
   {
      public int PageNumber { get; set; }
      public int PageSize { get; set; }
      public long TotalCount { get; set; } 
      public int TotalPages { get; set; }
      public List<T>? Items { get; set; }
   }
}
