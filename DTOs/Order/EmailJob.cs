namespace dotnet_backend.DTOs.Order
{
    public class EmailJob
    {
        public int OrderId {get; set;}
        public string Email {get; set;} = "";
        public byte[]  PdfBytes {get; set;}
        public int RetryCount {get; set;}

    }
}

