using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

builder.Services.AddOcelot();
builder.Services.AddHttpClient();
builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowFrontend",
        policy =>
        {
            policy
                .WithOrigins("http://localhost:5246", "http://localhost:8081")
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    );
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.DocInclusionPredicate(
        (docName, apiDesc) =>
        {
            var path = apiDesc.RelativePath ?? "";

            if (path.StartsWith("configuration"))
                return false;

            if (path.StartsWith("outputcache"))
                return false;

            return true;
        }
    );
});

var app = builder.Build();

app.UseCors("AllowFrontend");

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.DefaultModelsExpandDepth(-1);
});

app.MapControllers();

app.MapWhen(
    context => context.Request.Path.StartsWithSegments("/gateway"),
    gatewayApp =>
    {
        gatewayApp.UseOcelot().Wait();
    }
);

app.Run();
