namespace dotnet_backend.DTOs.Report
{
   public class RevenueByMothRequest
   {
      public string? StartDate { get; set; }
      public string? EndDate { get; set; }
   }
   public class MonthlyRevenueData
   {
      public int Month { get; set; }
      public decimal Revenue { get; set; }
      public int Orders { get; set; }
   }
   public class RevenueByMothResponse
   {
      public int Year { get; set; }
      public List<MonthlyRevenueData>? MonthlyRevenues { get; set; }
   }
}
