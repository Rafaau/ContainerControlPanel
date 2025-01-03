using ContainerControlPanel.Domain.Models;
using Refit;

namespace ContainerControlPanel.Web.Interfaces;

public interface ITelemetryAPI
{
    [Get("/v1/getTraces?resource={resource}&timestamp={timestamp}&timeOffset={timeOffset}")]
    Task<List<TracesRoot>> GetTraces(int timeOffset, string? resource = null, string? timestamp = null);

    [Get("/v1/getMetrics")]
    Task<List<MetricsRoot>> GetMetrics();

    [Get("/v1/getTrace?traceId={traceId}")]
    Task<List<TracesRoot>> GetTrace(string traceId);

    [Get("/v1/getLogs?timestamp={timestamp}&resource={resource}&timeOffset={timeOffset}")]
    Task<List<LogsRoot>> GetLogs(int timeOffset, string? timestamp = "null", string? resource = "all");

    [Get("/v1/getLogs?traceId={traceId}")]
    Task<LogsRoot> GetLog(string traceId);

    [Get("/v1/getRequestAndResponse?traceId={traceId}")]
    Task<ApiResponse<RequestResponse>> GetRequestAndResponse(string traceId);
}