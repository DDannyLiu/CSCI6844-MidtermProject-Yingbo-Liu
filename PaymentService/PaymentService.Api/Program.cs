using System.Text;
using Microsoft.EntityFrameworkCore;
using PaymentService.Api.Data;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

var builder = WebApplication.CreateBuilder(args);

// ================= DB =================
builder.Services.AddDbContext<PaymentDbContext>(options =>
    options.UseSqlite("Data Source=/app/Data/payments.db")
);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 自动迁移数据库
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
    db.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

Console.WriteLine("PaymentService starting...");

// ================= RabbitMQ 配置 =================

// 从环境变量读取 RabbitMQ 主机（docker-compose 会传 rabbitmq）
var rabbitHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost";

// 从环境变量读取账号密码
var rabbitUser = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? "guest";
var rabbitPass = Environment.GetEnvironmentVariable("RABBITMQ_PASS") ?? "guest";

// 创建连接工厂
var factory = new ConnectionFactory()
{
    HostName = rabbitHost,
    UserName = rabbitUser,
    Password = rabbitPass,
};

// ================= 关键修复：重试连接 RabbitMQ =================

IConnection connection = null!;
IModel channel = null!;

// 尝试最多 10 次连接 RabbitMQ
for (int i = 0; i < 10; i++)
{
    try
    {
        Console.WriteLine($"Connecting to RabbitMQ... attempt {i + 1}");

        // 尝试建立连接
        connection = factory.CreateConnection();

        // 创建 channel
        channel = connection.CreateModel();

        Console.WriteLine("Connected to RabbitMQ!");

        break;
    }
    catch (Exception ex)
    {
        // 如果 RabbitMQ 还没 ready，会进入这里
        Console.WriteLine($"Failed to connect: {ex.Message}");

        // 等 5 秒再试
        Thread.Sleep(5000);
    }
}

// 如果最终还是连接失败 → 直接报错退出
if (connection == null || channel == null)
{
    throw new Exception("Could not connect to RabbitMQ after multiple attempts.");
}

// ================= Queue 声明 =================

// 必须和 OrderService 使用同一个 queue 名
channel.QueueDeclare(
    queue: "orders-queue",
    durable: true,
    exclusive: false,
    autoDelete: false,
    arguments: null
);

Console.WriteLine("Waiting for orders...");

// ================= Consumer =================

// 创建消费者
var consumer = new EventingBasicConsumer(channel);

// 收到消息时执行
consumer.Received += (model, ea) =>
{
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);

    // CHANGED: 更通用，因为现在不只 OrderCreated，还有 OrderCancelled
    Console.WriteLine($"[PaymentService] Event received: {message}");

    // 手动 ack（确认消息已消费）
    channel.BasicAck(ea.DeliveryTag, false);
};

// 开始监听 queue
channel.BasicConsume(queue: "orders-queue", autoAck: false, consumer: consumer);

app.Run();
