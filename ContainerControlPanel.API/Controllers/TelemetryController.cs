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
    public IActionResult CollectTelemetry([FromBody] dynamic telemetryData)
    {
        _logger.LogInformation("Received telemetry data: {Data}", [telemetryData]);
        return Ok();
    }

    [HttpPost("traces")]
    public async Task<IActionResult> CollectTrace()
    {
        try
        {
            MessageBindable<ExportTraceServiceRequest>? bindable = 
                await MessageBindable<ExportTraceServiceRequest>.BindAsync(HttpContext, null);

            await _redisService.SetValueAsync($"trace{Guid.NewGuid().ToString()}", bindable?.Message.ToString(), TimeSpan.FromDays(14));
            await TelemetryWebSocketHandler.BroadcastMessageAsync(bindable?.Message.ToString());
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
        List<Root> traces = new();
        var result = await _redisService.ScanKeysByPatternAsync("trace");

        foreach (var item in result)
        {
            var deserialized = JsonSerializer.Deserialize<Root>(item);
            traces.Add(deserialized);
        }

        return Ok(traces);
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
