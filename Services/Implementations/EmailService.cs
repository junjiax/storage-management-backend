using dotnet_backend.Data;
using dotnet_backend.DTOs.Order;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

public class EmailService
{

    public async Task SendInvoiceAsync(EmailJob job)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Cửa hàng bán lẻ", "xvideosdm@gmail.com"));
        message.To.Add(new MailboxAddress("", job.Email));
        message.Subject = $"Hóa đơn #{job.OrderId}";

        var builder = new BodyBuilder
        {
            TextBody = "Cảm ơn bạn đã mua hàng!"
        };

        builder.Attachments.Add($"HoaDon_{job.OrderId}.pdf", job.PdfBytes);
        message.Body = builder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync("xvideosdm@gmail.com", "obtv ofxj pejf mubo");
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
