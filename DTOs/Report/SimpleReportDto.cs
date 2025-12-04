namespace dotnet_backend.DTOs.Report
{
   public class SimpleReportRequest
   {
      public DateTime? StartDate { get; set; }
      public DateTime? EndDate { get; set; }
   }
   public class SimpleReportResponse
   {
      public decimal TotalRevenue { get; set; }
      public int TotalOrders { get; set; }
      public int NewCustomers { get; set; }
      public double GrowthRate { get; set; }
   }

}
