namespace dotnet_backend.DTOs.Report
{
   public class SimpleReportRequest
   {
      public string? StartDate { get; set; }
      public string? EndDate { get; set; }
   }
   public class SimpleReportResponse
   {
      public decimal TotalRevenue { get; set; }
      public int TotalOrders { get; set; }
      public int NewCustomers { get; set; }
      public double GrowthRate { get; set; }
   }

}
