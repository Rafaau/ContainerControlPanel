using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Proto.Collector.Trace.V1;
using ContainerControlPanel.Domain.Models;
using System.Text.Json;
using OpenTelemetry.Proto.Collector.Metrics.V1;
using OpenTelemetry.Proto.Collector.Logs.V1;
using ContainerControlPanel.API.Models;

/// <summary>
/// Controller for handling telemetry data.
/// </summary>
[ApiController]
[Route("/v1")]
public class TelemetryController : ControllerBase
{
    /// <summary>
    /// Redis service 
    /// </summary>
    private readonly RedisService _redisService;

    /// <summary>
    /// Initializes a new instance of the <see cref="TelemetryController"/> class.
    /// </summary>
    /// <param name="redisService">Redis service</param>
    public TelemetryController(RedisService redisService)
    {
        _redisService = redisService;
    }

    /// <summary>
    /// Collects OpenTelemetry metrics data.
    /// </summary>
    /// <returns>Returns an action result</returns>
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
            string? routeName = metricsRoot?.GetRouteName();

            string json = JsonSerializer.Serialize(message);
            
            if (serviceName != null 
                && routeName != null)
            {
                await _redisService.SetValueAsync($"metrics{serviceName}{routeName}", bindable?.Message.ToString(), TimeSpan.FromDays(14));
                await TelemetryWebSocketHandler.BroadcastMessageAsync(json);
            }
                 
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Collects OpenTelemetry trace data.
    /// </summary>
    /// <returns>Returns an action result</returns>
    [HttpPost("traces")]
    public async Task<IActionResult> CollectTrace()
    {
        try
        {
            MessageBindable<ExportTraceServiceRequest>? bindable = 
                await MessageBindable<ExportTraceServiceRequest>.BindAsync(HttpContext, null);

            TracesRoot tracesRoot = JsonSerializer.Deserialize<TracesRoot>(bindable?.Message.ToString());

            foreach (var span in tracesRoot.GetSpans())
            {
                if (span.Name == "GET /docs")
                    continue;

                tracesRoot.ResourceSpans[0].ScopeSpans.Clear();
                tracesRoot.ResourceSpans[0].ScopeSpans.Add(new ScopeSpan
                {
                    Scope = new Scope { Name = "System.Net.Http" },
                    Spans = new List<Span> { span }
                });

                var serializedData = JsonSerializer.Serialize(tracesRoot);

                var message = new WebSocketMessage
                {
                    Type = WebSocketMessageType.Traces,
                    Data = tracesRoot
                };

                string json = JsonSerializer.Serialize(message);
                await _redisService.SetValueAsync($"trace{span.TraceId}{Guid.NewGuid().ToString()}", serializedData, TimeSpan.FromDays(14));      
                await TelemetryWebSocketHandler.BroadcastMessageAsync(json);
            }
           
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Collects OpenTelemetry logs data.
    /// </summary>
    /// <returns>Returns an action result</returns>
    [HttpPost("logs")]
    public async Task<IActionResult> CollectLogs()
    {
        try
        {
            MessageBindable<ExportLogsServiceRequest>? bindable =
                await MessageBindable<ExportLogsServiceRequest>.BindAsync(HttpContext, null);

            var message = new WebSocketMessage
            {
                Type = WebSocketMessageType.Logs,
                Data = bindable?.Message.ToString()
            };

            LogsRoot logsRoot = JsonSerializer.Deserialize<LogsRoot>(bindable?.Message.ToString());

            foreach (var logRecord in logsRoot.GetLogRecords())
            {
                await _redisService.SetValueAsync($"log{logRecord.TraceId}", bindable?.Message.ToString(), TimeSpan.FromDays(14));
            }

            string json = JsonSerializer.Serialize(message);
            await TelemetryWebSocketHandler.BroadcastMessageAsync(json);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Gets the stored traces.
    /// </summary>
    /// <param name="timeOffset">Time offset</param>
    /// <param name="resource">Resource name</param>
    /// <param name="timestamp">Timestamp</param>
    /// <returns>Returns the stored traces</returns>
    [HttpGet("GetTraces")]
    public async Task<IActionResult> GetTraces(int timeOffset, string? resource, string? timestamp)
    {
        List<TracesRoot> traces = new();
        var result = await _redisService.ScanKeysByPatternAsync("trace");

        foreach (var item in result)
        {
            var deserialized = JsonSerializer.Deserialize<TracesRoot>(item);

            if ((resource == "all" || deserialized.HasResource(resource))
                && (string.IsNullOrEmpty(timestamp) || deserialized.GetTimestamp(timeOffset).Date == DateTime.Parse(timestamp).Date))
                traces.Add(deserialized);
        }

        return Ok(traces);
    }

    /// <summary>
    /// Gets the stored trace.
    /// </summary>
    /// <param name="traceId">Trace ID</param>
    /// <returns>Returns the stored trace</returns>
    [HttpGet("GetTrace")]
    public async Task<IActionResult> GetTrace(string traceId)
    {
        List<TracesRoot> traces = new();
        var result = await _redisService.ScanKeysByPatternAsync($"trace{traceId}");

        if (result.Count == 0)
        {
            return NotFound();
        }

        foreach (var item in result)
        {
            var deserialized = JsonSerializer.Deserialize<TracesRoot>(item);
            traces.Add(deserialized);
        }

        return Ok(traces);
    }

    /// <summary>
    /// Gets the stored metrics.
    /// </summary>
    /// <returns>Returns the stored metrics</returns>
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

    /// <summary>
    /// Gets the stored logs.
    /// </summary>
    /// <param name="timeOffset">Time offset</param>
    /// <param name="timestamp">Timestamp</param>
    /// <param name="resource">Resource name</param>
    /// <returns>Returns the stored logs</returns>
    [HttpGet("GetLogs")]
    public async Task<IActionResult> GetLogs(int timeOffset, string? timestamp, string? resource)
    {
        List<LogsRoot> logs = new();
        var result = await _redisService.ScanKeysByPatternAsync("log");
        foreach (var item in result)
        {
            var deserialized = JsonSerializer.Deserialize<LogsRoot>(item);

            if ((resource == "all" || resource == deserialized.GetResourceName())
                && (timestamp == "null" || deserialized.GetTimestamp(timeOffset).Date == DateTime.Parse(timestamp).Date))
                logs.Add(deserialized);
        }
        return Ok(logs);
    }

    /// <summary>
    /// Gets the stored log.
    /// </summary>
    /// <param name="traceId">Trace ID</param>
    /// <returns>Returns the stored log</returns>
    [HttpGet("GetLog")]
    public async Task<IActionResult> GetLog(string traceId)
    {
        var result = await _redisService.ScanKeysByPatternAsync($"log{traceId}");
        if (result.Count == 0)
        {
            return NotFound();
        }
        var deserialized = JsonSerializer.Deserialize<LogsRoot>(result[0]);
        return Ok(deserialized);
    }

    /// <summary>
    /// Gets the request and response for a trace.
    /// </summary>
    /// <param name="traceId">Trace ID</param>
    /// <returns>Returns the request and response for the trace</returns>
    [HttpGet("GetRequestAndResponse")]
    public async Task<IActionResult> GetRequestAndResponse(string traceId)
    {
        var result = await _redisService.ScanKeysByPatternAsync($"log{traceId}");
        if (result.Count == 0)
        {
            return NotFound();
        }
        var deserialized = JsonSerializer.Deserialize<LogsRoot>(result[0]);
        RequestResponse reqRes = deserialized.GetRequestResponse();

        if (reqRes == null)
        {
            return NotFound();
        }

        return Ok(reqRes);
    }
}
