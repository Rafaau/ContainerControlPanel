using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Proto.Collector.Trace.V1;
using ContainerControlPanel.Domain.Models;
using System.Text.Json;
using OpenTelemetry.Proto.Collector.Metrics.V1;
using OpenTelemetry.Proto.Collector.Logs.V1;
using ContainerControlPanel.API.Models;
using ContainerControlPanel.API.Authorization;
using ContainerControlPanel.API.Interfaces;

/// <summary>
/// Controller for handling telemetry data.
/// </summary>
[ApiController]
[Route("/v1")]
public class TelemetryController : ControllerBase
{
    private readonly IDataStoreService _dataStoreService;

    /// <summary>
    /// Initializes a new instance of the <see cref="TelemetryController"/> class.
    /// </summary>
    /// <param name="redisService">Redis service</param>
    public TelemetryController(IDataStoreService dataStoreService)
    {
        _dataStoreService = dataStoreService;
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
                await _dataStoreService.SaveMetricsAsync(metricsRoot, serviceName, routeName);               
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

                var message = new WebSocketMessage
                {
                    Type = WebSocketMessageType.Traces,
                    Data = tracesRoot
                };

                string json = JsonSerializer.Serialize(message);
                await _dataStoreService.SaveTraceAsync(tracesRoot, span.TraceId);
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
            logsRoot.ResourceName = logsRoot.GetResourceName();

            foreach (var logRecord in logsRoot.GetLogRecords())
            {
                logsRoot.Severity = logRecord.SeverityText;
                logsRoot.Message = logRecord.Body.StringValue;
                await _dataStoreService.SaveLogAsync(logsRoot, logRecord.TraceId);
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
    [TokenAuthorize]
    public async Task<IActionResult> GetTraces(int timeOffset, string? resource, string? timestamp)
    {
        try
        {
            List<TracesRoot> traces = await _dataStoreService.GetTracesAsync(timeOffset, resource, timestamp);

            return Ok(traces);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Gets the stored trace.
    /// </summary>
    /// <param name="traceId">Trace ID</param>
    /// <returns>Returns the stored trace</returns>
    [HttpGet("GetTrace")]
    [TokenAuthorize]
    public async Task<IActionResult> GetTrace(string traceId)
    {
        try
        {
            List<TracesRoot> traces = await _dataStoreService.GetTraceAsync(traceId);
            return Ok(traces);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Gets the stored metrics.
    /// </summary>
    /// <returns>Returns the stored metrics</returns>
    [HttpGet("GetMetrics")]
    [TokenAuthorize]
    public async Task<IActionResult> GetMetrics()
    {
        try
        {
            List<MetricsRoot> metrics = await _dataStoreService.GetMetricsAsync();
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Gets the stored logs.
    /// </summary>
    /// <param name="timeOffset">Time offset</param>
    /// <param name="timestamp">Timestamp</param>
    /// <param name="resource">Resource name</param>
    /// <returns>Returns the stored logs</returns>
    [HttpGet("GetLogs")]
    [TokenAuthorize]
    public async Task<IActionResult> GetLogs(
        int timeOffset, 
        string? timestamp, 
        string? resource,
        string? severity,
        string? filter,
        int page = 0, 
        int pageSize = 0)
    {
        try
        {
            List<LogsRoot> logs = await _dataStoreService.GetLogsAsync(
                timeOffset, 
                timestamp, 
                resource,
                severity,
                filter,
                page, 
                pageSize);
            return Ok(logs);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Gets the stored log.
    /// </summary>
    /// <param name="traceId">Trace ID</param>
    /// <returns>Returns the stored log</returns>
    [HttpGet("GetLog")]
    [TokenAuthorize]
    public async Task<IActionResult> GetLog(string traceId)
    {
        try
        {
            LogsRoot log = await _dataStoreService.GetLogAsync(traceId);
            return Ok(log);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Gets the request and response for a trace.
    /// </summary>
    /// <param name="traceId">Trace ID</param>
    /// <returns>Returns the request and response for the trace</returns>
    [HttpGet("GetRequestAndResponse")]
    [TokenAuthorize]
    public async Task<IActionResult> GetRequestAndResponse(string traceId)
    {
        LogsRoot log = await _dataStoreService.GetLogAsync(traceId);
        RequestResponse reqRes = log.GetRequestResponse();

        if (reqRes == null)
        {
            return NotFound();
        }

        return Ok(reqRes);
    }
}
