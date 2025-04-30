using ContainerControlPanel.API.Authorization;
using ContainerControlPanel.API.Interfaces;
using ContainerControlPanel.API.Services;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureAppConfiguration((hostingContext, config) =>
{
    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
    config.AddEnvironmentVariables();
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy.WithOrigins($"http://{builder.Configuration["WebApp:Host"]}:{builder.Configuration["WebApp:Port"]}") 
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddMemoryCache();
#pragma warning disable EXTEXP0018
builder.Services.AddHybridCache();
#pragma warning restore EXTEXP0018

if (string.IsNullOrEmpty(builder.Configuration["MongoDB:ConnectionString"]))
{
    builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(builder.Configuration["Redis:ConnectionString"]));
    builder.Services.AddSingleton<RedisService>();
}
else
{
    builder.Services.AddSingleton<IDataStoreService, MongoService>();
}

builder.Services.AddSingleton<SessionValidation>();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseWebSockets();
app.Use(async (context, next) =>
{
    if (context.Request.Path == "/ws")
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            await WebSocketHandler.HandleWebSocketConnectionAsync(webSocket);
        }
        else
        {
            context.Response.StatusCode = 400; // Bad Request
        }
    }
    else
    {
        await next();
    }
});

if (!string.IsNullOrEmpty(builder.Configuration["MongoDB:ConnectionString"]))
{
    var mongoService = new MongoService(app.Configuration);
    await mongoService.InitializeAsync();
}

app.UseCors("AllowBlazorClient");

app.UseRouting();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();

