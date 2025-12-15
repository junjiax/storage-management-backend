using RabbitMQ.Client;
using System.Text;

namespace dotnet_backend.RabbitMQ
{
    public class RabbitPublisher
    {
        private readonly IConnectionFactory _factory;

        public RabbitPublisher(string host = "localhost", int port = 5672, string user = "guest", string pass = "guest")
        {
            _factory = new ConnectionFactory
            {
                HostName = host,
                Port = port,
                UserName = user,
                Password = pass
            };
        }

        public async Task PublishAsync(string queueName, string message)
        {
            // Tạo connection
            await using var connection = await _factory.CreateConnectionAsync();
            // Tạo channel
            await using var channel = await connection.CreateChannelAsync();
            try
            {
                await channel.QueueDeclarePassiveAsync(queueName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Queue '{queueName}' does not exist: {ex.Message}");
                throw new InvalidOperationException($"Queue '{queueName}' does not exist. Please run RabbitMqInitializer first.");
            }

            // Tạo BasicProperties theo API mới
            var props = new BasicProperties
            {
                ContentType = "application/json",
                DeliveryMode = DeliveryModes.Persistent // Lưu message vào disk
            };

            var body = Encoding.UTF8.GetBytes(message);

            // Gửi message
            await channel.BasicPublishAsync(
                exchange: "",
                routingKey: queueName,
                mandatory: false,
                props,
                body
            );
        }
    }
}