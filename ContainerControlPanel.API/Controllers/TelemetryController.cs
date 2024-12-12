using Microsoft.AspNetCore.Mvc;
using Google.Protobuf;
using System.Reflection;
using Microsoft.Extensions.Primitives;
using System.Net.Http.Headers;
using OpenTelemetry.Proto.Collector.Trace.V1;
using System.Buffers;
using System.IO.Pipelines;
using ContainerControlPanel.Domain.Models;
using System.Text;
using System;
using System.Text.Json;
using OpenTelemetry.Proto.Collector.Metrics.V1;

[ApiController]
[Route("/v1")]
public class TelemetryController : ControllerBase
{
    private readonly ILogger<TelemetryController> _logger;
    private readonly RedisService _redisService;

    public const string ProtobufContentType = "application/x-protobuf";
    public const string JsonContentType = "application/json";
    public const string CorsPolicyName = "OtlpHttpCors";

    public TelemetryController(ILogger<TelemetryController> logger, RedisService redisService)
    {
        _logger = logger;
        _redisService = redisService;
    }

    [HttpPost("metrics")]
    public async Task<IActionResult> CollectTelemetry()
    {
        try
        {
            MessageBindable<ExportMetricsServiceRequest>? bindable =
                await MessageBindable<ExportMetricsServiceRequest>.BindAsync(HttpContext, null);

            var message = new WebSocketMessage
            {
                Type = WebSocketMessageType.Metrics,
                Data = bindable?.Message.ToString()
            };

            MetricsRoot metricsRoot = JsonSerializer.Deserialize<MetricsRoot>(bindable?.Message.ToString());
            string? serviceName = metricsRoot?.GetResource()?.GetResourceName();

            string json = JsonSerializer.Serialize(message);
            
            if (serviceName != null)
            {
                await _redisService.SetValueAsync($"metrics{serviceName}", bindable?.Message.ToString(), TimeSpan.FromDays(14));
                await TelemetryWebSocketHandler.BroadcastMessageAsync(json);
            }
                 
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("traces")]
    public async Task<IActionResult> CollectTrace()
    {
        try
        {
            MessageBindable<ExportTraceServiceRequest>? bindable = 
                await MessageBindable<ExportTraceServiceRequest>.BindAsync(HttpContext, null);

            var message = new WebSocketMessage
            {
                Type = WebSocketMessageType.Traces,
                Data = bindable?.Message.ToString()
            };

            string json = JsonSerializer.Serialize(message);

            //await _redisService.SetValueAsync($"trace{Guid.NewGuid().ToString()}", bindable?.Message.ToString(), TimeSpan.FromDays(14));
            //await TelemetryWebSocketHandler.BroadcastMessageAsync(json);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("GetTraces")]
    public async Task<IActionResult> GetTraces()
    {
        List<TracesRoot> traces = new();
        var result = await _redisService.ScanKeysByPatternAsync("trace");

        foreach (var item in result)
        {
            var deserialized = JsonSerializer.Deserialize<TracesRoot>(item);
            traces.Add(deserialized);
        }

        return Ok(traces);
    }

    [HttpGet("GetMetrics")]
    public async Task<IActionResult> GetMetrics()
    {
        List<MetricsRoot> metrics = new();
        var result = await _redisService.ScanKeysByPatternAsync("metrics");
        foreach (var item in result)
        {
            var deserialized = JsonSerializer.Deserialize<MetricsRoot>(item);
            metrics.Add(deserialized);
        }
        return Ok(metrics);
    }

    public sealed class MessageBindable<TMessage> : IBindableFromHttpContext<MessageBindable<TMessage>> where TMessage : IMessage<TMessage>, new()
    {
        public static readonly MessageBindable<TMessage> Empty = new MessageBindable<TMessage>();

        public TMessage? Message { get; private set; }

        public static async ValueTask<MessageBindable<TMessage>?> BindAsync(HttpContext context, ParameterInfo parameter)
        {
            switch (GetKnownContentType(context.Request.ContentType, out var charSet))
            {
                case KnownContentType.Protobuf:
                    try
                    {
                        var message = await ReadOtlpData(context, data =>
                        {
                            var message = new TMessage();
                            message.MergeFrom(data);
                            return message;
                        }).ConfigureAwait(false);

                        return new MessageBindable<TMessage> { Message = message };
                    }
                    catch (BadHttpRequestException ex)
                    {
                        context.Response.StatusCode = ex.StatusCode;
                        return Empty;
                    }
                case KnownContentType.Json:
                default:
                    context.Response.StatusCode = StatusCodes.Status415UnsupportedMediaType;
                    return Empty;
            }
        }

        private static KnownContentType GetKnownContentType(string? contentType, out StringSegment charSet)
        {
            if (contentType != null && MediaTypeHeaderValue.TryParse(contentType, out var mt))
            {
                if (string.Equals(mt.MediaType, JsonContentType, StringComparison.OrdinalIgnoreCase))
                {
                    charSet = mt.CharSet;
                    return KnownContentType.Json;
                }

                if (string.Equals(mt.MediaType, ProtobufContentType, StringComparison.OrdinalIgnoreCase))
                {
                    charSet = mt.CharSet;
                    return KnownContentType.Protobuf;
                }
            }

            charSet = default;
            return KnownContentType.None;
        }

        private static async Task<T> ReadOtlpData<T>(HttpContext httpContext, Func<ReadOnlySequence<byte>, T> exporter)
        {
            const int MaxRequestSize = 1024 * 1024 * 4; // 4 MB. Matches default gRPC request limit.

            ReadResult result = default;
            try
            {
                do
                {
                    result = await httpContext.Request.BodyReader.ReadAsync().ConfigureAwait(false);

                    if (result.IsCanceled)
                    {
                        throw new OperationCanceledException("Read call was canceled.");
                    }

                    if (result.Buffer.Length > MaxRequestSize)
                    {
                        // Too big!
                        throw new BadHttpRequestException(
                            $"The request body was larger than the max allowed of {MaxRequestSize} bytes.",
                            StatusCodes.Status400BadRequest);
                    }

                    if (result.IsCompleted)
                    {
                        break;
                    }
                    else
                    {
                        httpContext.Request.BodyReader.AdvanceTo(result.Buffer.Start, result.Buffer.End);
                    }
                } while (true);

                return exporter(result.Buffer);
            }
            finally
            {
                if (!result.Equals(default(ReadResult)))
                {
                    httpContext.Request.BodyReader.AdvanceTo(result.Buffer.End);
                }
            }
        }

        private enum KnownContentType
        {
            None,
            Protobuf,
            Json
        }
    }
}
