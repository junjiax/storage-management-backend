using RabbitMQ.Client;

namespace dotnet_backend.DTOs.Order
{
    public class RabbitMqInitializer
    {
        public static async Task InitializeAsync()
        {
            var factory = new ConnectionFactory()
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest"
            };

            await using var connection = await factory.CreateConnectionAsync();
            await using var channel = await connection.CreateChannelAsync();

            try
            {
                // 1. Exchange chính — nơi producer publish email
                await channel.ExchangeDeclareAsync(
                    exchange: "email_exchange",
                    type: ExchangeType.Direct,
                    durable: true
                );

                // 2. Exchange retry — nơi email_queue chuyển lỗi qua
                await channel.ExchangeDeclareAsync(
                    exchange: "email_retry_exchange",
                    type: ExchangeType.Direct,
                    durable: true
                );

                // 3. DLX (Dead Letter Exchange)
                await channel.ExchangeDeclareAsync(
                    exchange: "email_dlx",
                    type: ExchangeType.Direct,
                    durable: true
                );

                // 4. Queue chính (email_queue)
                await channel.QueueDeclareAsync(
                    queue: "email_queue",
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: new Dictionary<string, object>
                    {
                        { "x-dead-letter-exchange", "email_retry_exchange" },
                        { "x-dead-letter-routing-key", "email_retry" }
                    }
                );

                // 5. Queue retry (delay)
                await channel.QueueDeclareAsync(
                    queue: "email_retry_queue",
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: new Dictionary<string, object>
                    {
                        { "x-message-ttl", 10000 }, // delay 10s
                        { "x-dead-letter-exchange", "email_exchange" },
                        { "x-dead-letter-routing-key", "email" }
                    }
                );

                // 6. DLQ queue
                await channel.QueueDeclareAsync(
                    queue: "email_dlq",
                    durable: true,
                    exclusive: false,
                    autoDelete: false
                );

                // 7. Bind queues vào các exchange
                await channel.QueueBindAsync("email_queue", "email_exchange", "email");
                await channel.QueueBindAsync("email_retry_queue", "email_retry_exchange", "email_retry");
                await channel.QueueBindAsync("email_dlq", "email_dlx", "email_dlq");

                Console.WriteLine("RabbitMQ initialized (async API).");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"RabbitMQ initialization error: {ex.Message}");
                throw;
            }
        }
    }
}
