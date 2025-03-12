using ContainerControlPanel.Domain.Models;
using Refit;

namespace ContainerControlPanel.Web.Interfaces;

/// <summary>
/// Telemetry API Refit interface.
/// </summary>
public interface ITelemetryAPI
{
    /// <summary>
    /// Gets the traces.
    /// </summary>
    /// <param name="timeOffset">Time offset.</param>
    /// <param name="resource">Resource name.</param>
    /// <param name="timestamp">Timestamp.</param>
    /// <returns>Returns the list of traces.</returns>
    [Get("/v1/getTraces?resource={resource}&timestamp={timestamp}&timeOffset={timeOffset}&page={page}&pageSize={pageSize}")]
    Task<List<Trace>> GetTraces(int timeOffset, string? resource = null, string? timestamp = null, int page = 0, int pageSize = 0);

    /// <summary>
    /// Gets the metrics.
    /// </summary>
    /// <returns>Returns the list of metrics.</returns>
    [Get("/v1/getMetrics")]
    Task<List<MetricsRoot>> GetMetrics();

    /// <summary>
    /// Gets the trace.
    /// </summary>
    /// <param name="traceId">Trace ID.</param>
    /// <returns>Returns the list of traces.</returns
    [Get("/v1/getTrace?traceId={traceId}")]
    Task<List<TracesRoot>> GetTrace(string traceId);

    /// <summary>
    /// Gets the logs.
    /// </summary>
    /// <param name="timeOffset">Time offset.</param>
    /// <param name="timestamp">Timestamp.</param>
    /// <param name="resource">Resource name.</param>
    /// <returns>Returns the list of logs.</returns>
    [Get("/v1/getLogs?timestamp={timestamp}&resource={resource}&timeOffset={timeOffset}&severity={severity}&filter={filter}&page={page}&pageSize={pageSize}")]
    Task<List<Log>> GetLogs(
        int timeOffset, 
        string? timestamp = "null", 
        string? resource = "all",
        string? severity = "all",
        string? filter = "",
        int page = 0, 
        int pageSize = 0);

    /// <summary>
    /// Gets the log.
    /// </summary>
    /// <param name="traceId">Trace ID.</param>
    /// <returns>Returns the log.</returns>
    [Get("/v1/getLogs?traceId={traceId}")]
    Task<Log> GetLog(string traceId);

    /// <summary>
    /// Gets the request and response.
    /// </summary>
    /// <param name="traceId">Trace ID.</param>
    /// <returns>Returns the request and response.</returns>
    [Get("/v1/getRequestAndResponse?traceId={traceId}")]
    Task<ApiResponse<RequestResponse>> GetRequestAndResponse(string traceId);
}