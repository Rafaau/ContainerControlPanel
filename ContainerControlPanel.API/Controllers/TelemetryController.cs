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
    /// <summary>
    /// Data store service
    /// </summary>
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
           
            MetricsRoot metricsRoot = JsonSerializer.Deserialize<MetricsRoot>(bindable?.Message.ToString());
            string? serviceName = metricsRoot?.GetResource()?.GetResourceName();
            string? routeName = metricsRoot?.GetRouteName();
     
            if (serviceName != null 
                && routeName != null)
            {
                Metrics metrics = metricsRoot.GetMetrics();

                var message = new WebSocketMessage
                {
                    Type = WebSocketMessageType.Metrics,
                    Data = metrics
                };
                string json = JsonSerializer.Serialize(message);

                await _dataStoreService.SaveMetricsAsync(metrics, serviceName, routeName);               
                await WebSocketHandler.BroadcastMessageAsync(json);
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

                Trace trace = tracesRoot.GetTrace(span);

                var message = new WebSocketMessage
                {
                    Type = WebSocketMessageType.Traces,
                    Data = trace
                };
                string json = JsonSerializer.Serialize(message);

                await _dataStoreService.SaveTraceAsync(trace, span.TraceId);
                await WebSocketHandler.BroadcastMessageAsync(json);
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

            LogsRoot logsRoot = JsonSerializer.Deserialize<LogsRoot>(bindable?.Message.ToString());

            foreach (var logRecord in logsRoot.GetLogRecords())
            {
                Log log = logsRoot.GetLog(logRecord);

                var message = new WebSocketMessage
                {
                    Type = WebSocketMessageType.Logs,
                    Data = log
                };
                string json = JsonSerializer.Serialize(message);

                await _dataStoreService.SaveLogAsync(log, logRecord.TraceId);         
                await WebSocketHandler.BroadcastMessageAsync(json);
            }
       
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
    public async Task<IActionResult> GetTraces(
        int timeOffset, 
        string? resource, 
        string? timestamp,
        bool routesOnly = false,
        int page = 0,
        int pageSize = 0)
    {
        try
        {
            List<Trace> traces = 
                await _dataStoreService.GetTracesAsync(timeOffset, resource, timestamp, routesOnly, page, pageSize);

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
            Trace trace = await _dataStoreService.GetTraceAsync(traceId);
            return Ok(trace);
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
            List<Metrics> metrics = await _dataStoreService.GetMetricsAsync();
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
            List<Log> logs = await _dataStoreService.GetLogsAsync(
                timeOffset, 
                timestamp, 
                resource,
                severity,
                filter,
                page, 
                pageSize);
            return Ok(logs.Where(log => !log.ContainsReqRes()));
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
    //[HttpGet("GetLog")]
    //[TokenAuthorize]
    //public async Task<IActionResult> GetLog(string traceId)
    //{
    //    try
    //    {
    //        Log log = await _dataStoreService.GetLogsByTraceAsync(traceId);
    //        return Ok(log);
    //    }
    //    catch (Exception ex)
    //    {
    //        return BadRequest(ex.Message);
    //    }
    //}

    /// <summary>
    /// Gets the request and response for a trace.
    /// </summary>
    /// <param name="traceId">Trace ID</param>
    /// <returns>Returns the request and response for the trace</returns>
    [HttpGet("GetRequestAndResponse")]
    [TokenAuthorize]
    public async Task<IActionResult> GetRequestAndResponse(string traceId)
    {
        List<Log> logs = await _dataStoreService.GetLogsByTraceAsync(traceId);
        RequestResponse reqRes = logs.GetRequestResponse();

        if (reqRes == null)
        {
            return NotFound();
        }

        return Ok(reqRes);
    }
}
