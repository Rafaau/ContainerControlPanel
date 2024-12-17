using ContainerControlPanel.Domain.Models;
using Refit;

namespace ContainerControlPanel.Web.Interfaces;

public interface ITelemetryAPI
{
    [Get("/v1/getTraces?resource={resource}")]
    Task<List<TracesRoot>> GetTraces(string? resource = null);

    [Get("/v1/getMetrics")]
    Task<List<MetricsRoot>> GetMetrics();

    [Get("/v1/getTrace?traceId={traceId}")]
    Task<TracesRoot> GetTrace(string traceId);

    [Get("/v1/getLogs")]
    Task<List<LogsRoot>> GetLogs();

    [Get("/v1/getLogs?traceId={traceId}")]
    Task<LogsRoot> GetLog(string traceId);

    [Get("/v1/getRequestAndResponse?traceId={traceId}")]
    Task<ApiResponse<RequestResponse>> GetRequestAndResponse(string traceId);
}