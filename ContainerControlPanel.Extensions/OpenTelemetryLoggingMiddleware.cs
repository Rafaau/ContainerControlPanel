using Microsoft.AspNetCore.Http;
using OpenTelemetry.Trace;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ContainerControlPanel.Extensions;

public class OpenTelemetryLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public OpenTelemetryLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        context.Request.EnableBuffering();

        string requestBody = string.Empty;
        string json = string.Empty;

        if (context.Request.ContentLength > 0 && context.Request.Body.CanRead)
        {
            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
            requestBody = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;           
        }

        var req = new
        {
            context.Request.Headers,
            context.Request.Query,
            Body = string.IsNullOrEmpty(requestBody)
                    ? null
                    : JsonObject.Parse(requestBody)
        };

        json = JsonSerializer.Serialize(req);

        var span = Tracer.CurrentSpan;
        span?.SetAttribute("http.request.body", json);

        var originalBody = context.Response.Body;
        using var newBody = new MemoryStream();
        context.Response.Body = newBody;

        await _next(context);

        newBody.Seek(0, SeekOrigin.Begin);
        string responseBody = await new StreamReader(newBody).ReadToEndAsync();
        newBody.Seek(0, SeekOrigin.Begin);
        await newBody.CopyToAsync(originalBody);

        span?.SetAttribute("http.response.body", responseBody);
    }
}
