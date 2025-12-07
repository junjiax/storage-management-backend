namespace dotnet_backend.DTOs.Report
{
      public class RatioByCategoryRequest
      {
         public string? StartDate { get; set; }
         public string? EndDate { get; set; }
      }

      public class RatioByCategoryData
      {
         public string Type { get; set; } = string.Empty;
         public decimal Value { get; set; }
      }

      public class RatioByCategoryResponse
      {
         public List<RatioByCategoryData>? Ratios { get; set; }
      }

}
