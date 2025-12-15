using System.Text;
using System.Threading;
using System.Threading.Tasks;
using dotnet_backend.DTOs.Order;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

public class EmailWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConnectionFactory _factory;

    public EmailWorker(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _factory = new ConnectionFactory { HostName = "localhost" };
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var connection = await _factory.CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();

        try
        {
            // CHỈ kiểm tra queue tồn tại, KHÔNG tạo lại
            await channel.QueueDeclarePassiveAsync("email_queue");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Queue 'email_queue' does not exist or error: {ex.Message}");
            Console.WriteLine("Please run RabbitMqInitializer first to create queues.");
            // Có thể throw hoặc chờ
            await Task.Delay(5000, stoppingToken);
            return; // Thoát và để HostedService retry
        }

        var consumer = new AsyncEventingBasicConsumer(channel);
        Console.WriteLine("Worker started, waiting for messages...");
        
        consumer.ReceivedAsync += async (sender, ea) =>
        {
            var body = Encoding.UTF8.GetString(ea.Body.ToArray());
            Console.WriteLine("Message received: " + body);
            var job = JsonConvert.DeserializeObject<EmailJob>(body)!;
            Console.WriteLine("Job email: " + job.Email);

            try
            {
                // Tạo scope mới cho mỗi message
                using (var scope = _serviceProvider.CreateScope())
                {
                    var emailService = scope.ServiceProvider.GetRequiredService<EmailService>();
                    await emailService.SendInvoiceAsync(job);
                }
                
                Console.WriteLine("Email sent for order: " + job.OrderId);
                await channel.BasicAckAsync(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {   
                Console.WriteLine($"Error sending email: {ex.Message}");
                job.RetryCount++;

                var retryBody = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(job));
                var props = new BasicProperties { DeliveryMode = DeliveryModes.Persistent };

                if (job.RetryCount >= 3)
                {
                    // DLQ
                    await channel.BasicPublishAsync<BasicProperties>(
                        exchange: "",
                        routingKey: "email_dlq",
                        mandatory: false,
                        basicProperties: props,
                        body: retryBody
                    );
                }
                else
                {
                    // Retry queue
                    await channel.BasicPublishAsync<BasicProperties>(
                        exchange: "email_retry_exchange",
                        routingKey: "email_retry",
                        mandatory: false,
                        basicProperties: props,
                        body: retryBody
                    );
                }

                await channel.BasicAckAsync(ea.DeliveryTag, false);
            }
        };

        await channel.BasicConsumeAsync("email_queue", false, consumer);

        // Giữ worker chạy
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}