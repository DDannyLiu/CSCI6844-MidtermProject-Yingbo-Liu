using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// NEW: 读取 Ocelot 配置文件
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// NEW: Ocelot services
builder.Services.AddOcelot();

// NEW: Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// NEW: 给 aggregation controller 用的 HttpClient
builder.Services.AddHttpClient();

// NEW: 启用 controller（比如 OrderDetailsController）
builder.Services.AddControllers();

var app = builder.Build();

// NEW: Swagger UI
app.UseSwagger();
app.UseSwaggerUI();

// NEW: 先映射你自己的 controller 路由
// 例如：/aggregate/orderdetails/{id}
app.MapControllers();

// IMPORTANT:
// 这里只让 Ocelot 处理 /gateway 开头的请求
// 这样 /aggregate/... 不会再被 Ocelot 抢走
app.MapWhen(
    context => context.Request.Path.StartsWithSegments("/gateway"),
    gatewayApp =>
    {
        gatewayApp.UseOcelot().Wait();
    }
);

app.Run();
