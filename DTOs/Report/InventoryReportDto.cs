namespace dotnet_backend.DTOs.Report
{
   public class InventoryReportRequest
   {
      public int PageNumber { get; set; } = 1;
      public int PageSize { get; set; } = 20;
   }

   public class LowStockReportResponse
   {
      public int ProductId { get; set; }
      public string ProductName { get; set; } = string.Empty;
      public int QuantityInStock { get; set; }
      public int SalesLast30Days{ get; set; }
      public int SuggestedReorderQty { get; set; }
   }
   public class InventoryReportReponse
   {
      public PaginatedResult<LowStockReportResponse>? InventoryItems { get; set; }
   }
}
