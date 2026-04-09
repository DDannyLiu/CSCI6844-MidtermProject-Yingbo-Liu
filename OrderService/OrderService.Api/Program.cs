using Microsoft.EntityFrameworkCore;
using OrderService.Api.Data;
using OrderService.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// EXISTING: keep your SQLite DbContext
builder.Services.AddDbContext<OrdersDbContext>(options =>
    options.UseSqlite("Data Source=/app/Data/orders.db")
);

// EXISTING: Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// EXISTING: HttpClient for CustomerService
builder.Services.AddHttpClient<ICustomerClient, CustomerClient>(client =>
{
    client.BaseAddress = new Uri("http://customerservice:8080/");
});

// EXISTING: HttpClient for ProductService
builder.Services.AddHttpClient<IProductClient, ProductClient>(client =>
{
    client.BaseAddress = new Uri("http://productservice:8080/");
});

var app = builder.Build();

// EXISTING: automatic migration on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
    db.Database.Migrate();
}

// CHANGED: enable Swagger for all environments
// 原来是 if (app.Environment.IsDevelopment()) 才开启
// 现在改成无条件开启，这样 Docker 里也能打开 Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "OrderService API V1");
    c.RoutePrefix = "swagger"; // CHANGED: Swagger UI available at /swagger
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
