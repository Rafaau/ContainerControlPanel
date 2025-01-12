using System.Text.Json.Nodes;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace ContainerControlPanel.Extensions;

/// <summary>
/// Extension methods for ILogger interface.
/// </summary>
public static class LoggerExtensions
{
    /// <summary>
    /// Log request details.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    /// <param name="request">HttpRequest instance.</param>
    public static void LogRequest(this ILogger logger, HttpRequest request)
    {
        string body = string.Empty;
        request.Body.Seek(0, SeekOrigin.Begin);

        using (StreamReader stream = new StreamReader(request.Body))
        {
            try
            {
                if (stream.EndOfStream == false)
                    body = stream.ReadToEnd();
            }
            catch { }

            var message = new
            {
                request.Headers,
                request.Query,
                Body = string.IsNullOrEmpty(body)
                    ? null
                    : JsonObject.Parse(body)
            };

            var json = JsonSerializer.Serialize(message);

            logger.LogInformation($"[REQUEST]{json}");
        }
    }

    /// <summary>
    /// Log response details.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    /// <param name="response">HttpResponse instance.</param>
    public static void LogResponse(this ILogger logger, object response)
    {
        var json = JsonSerializer.Serialize(response);
        logger.LogInformation($"[RESPONSE]{json}");
    }
}
