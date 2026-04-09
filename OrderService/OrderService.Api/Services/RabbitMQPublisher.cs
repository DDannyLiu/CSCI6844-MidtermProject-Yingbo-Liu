using System.Text;
using RabbitMQ.Client;

namespace OrderService.Api.Services
{
    public class RabbitMQPublisher
    {
        private readonly string _hostname;
        private readonly string _queueName;

        public RabbitMQPublisher(string hostname, string queueName)
        {
            // NEW: 从外部传入 hostname（支持 docker-compose）
            _hostname = hostname;

            _queueName = queueName;
        }

        public void Publish(string message)
        {
            // NEW: 创建连接工厂
            var factory = new ConnectionFactory()
            {
                // IMPORTANT:
                // 在 docker-compose 中，这里必须是 "rabbitmq"
                // 因为服务之间通过容器名通信
                HostName = _hostname,

                UserName = "guest",
                Password = "guest",
            };

            // NEW: 建立连接
            using var connection = factory.CreateConnection();

            // NEW: 创建 channel
            using var channel = connection.CreateModel();

            // NEW: 声明队列（producer 和 consumer 必须一致）
            channel.QueueDeclare(
                queue: _queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            // NEW: 将消息转为 byte[]
            var body = Encoding.UTF8.GetBytes(message);

            // NEW: 发送消息到队列
            channel.BasicPublish(
                exchange: "",
                routingKey: _queueName,
                basicProperties: null,
                body: body
            );

            // NEW: log（用于验证）
            Console.WriteLine($"[OrderService] Message published to queue: {_queueName}");
        }
    }
}
