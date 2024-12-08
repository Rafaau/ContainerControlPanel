using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy.WithOrigins("http://localhost:5069") 
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddMemoryCache();
#pragma warning disable EXTEXP0018
builder.Services.AddHybridCache();
#pragma warning restore EXTEXP0018

builder.Services.AddGrpc();
builder.Services.AddOpenTelemetry()
    .WithTracing(builder => builder.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation().AddConsoleExporter())
    .WithMetrics(builder => builder.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation().AddConsoleExporter());

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
            await TelemetryWebSocketHandler.HandleWebSocketConnectionAsync(webSocket);
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

app.UseCors("AllowBlazorClient");

app.UseRouting();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();

