using ContainerControlPanel.Domain.Models;

namespace ContainerControlPanel.API.Interfaces;

/// <summary>
/// Interface for managing data store service
/// </summary>
public interface IDataStoreService
{
    /// <summary>
    /// Gets a list of saved requests
    /// </summary>
    /// <returns>Returns a list of saved requests</returns>
    Task<List<SavedRequest>> GetRequestsAsync();

    /// <summary>
    /// Saves a request
    /// </summary>
    /// <param name="request">Request to save</param>
    Task SaveRequestAsync(SavedRequest request);

    /// <summary>
    /// Deletes a request
    /// </summary>
    /// <param name="id">Request ID</param>
    Task DeleteRequestAsync(string id);

    /// <summary>
    /// Pins a request
    /// </summary>
    /// <param name="id">Request ID</param>
    Task PinRequestAsync(string id);

    /// <summary>
    /// Saves metrics
    /// </summary>
    /// <param name="metrics">Metrics to save</param>
    /// <param name="serviceName">Name of the service</param>
    /// <param name="routeName">Name of the route</param>
    Task SaveMetricsAsync(MetricsRoot metrics, string? serviceName = "", string? routeName = "");

    /// <summary>
    /// Saves traces
    /// </summary>
    /// <param name="trace">Traces to save</param>
    /// <param name="traceId">Trace ID</param>
    Task SaveTraceAsync(TracesRoot trace, string? traceId = "");

    /// <summary>
    /// Saves logs
    /// </summary>
    /// <param name="log">Logs to save</param>
    /// <param name="traceId">Trace ID</param>
    Task SaveLogAsync(Log log, string? traceId = "");

    /// <summary>
    /// Gets a list of traces
    /// </summary>
    /// <param name="timeOffset">Time offset</param>
    /// <param name="resource">Name of the resource</param>
    /// <param name="timestamp">Timestamp</param>
    /// <param name="page">Number of the page</param>
    /// <param name="pageSize">Size of the page</param>
    /// <returns>Returns a list of traces</returns>
    Task<List<TracesRoot>> GetTracesAsync(int timeOffset, string? resource, string? timestamp, int page, int pageSize);

    /// <summary>
    /// Gets a trace
    /// </summary>
    /// <param name="traceId">Trace ID</param>
    /// <returns>Returns a trace</returns>
    Task<List<TracesRoot>> GetTraceAsync(string traceId);

    /// <summary>
    /// Gets a list of metrics
    /// </summary>
    /// <returns>Returns a list of metrics</returns>
    Task<List<MetricsRoot>> GetMetricsAsync();

    /// <summary>
    /// Gets a list of logs
    /// </summary>
    /// <param name="timeOffset">Time offset</param>
    /// <param name="timestamp">Timestamp</param>
    /// <param name="resource">Name of the resource</param>
    /// <param name="severity">Name of the severity</param>
    /// <param name="filter">String to filter</param>
    /// <param name="page">Number of the page</param>
    /// <param name="pageSize">Size of the page</param>
    /// <returns>Returns a list of logs</returns>
    Task<List<Log>> GetLogsAsync(
        int timeOffset, 
        string? timestamp, 
        string? resource, 
        string? severity, 
        string? filter,
        int page, 
        int pageSize);

    /// <summary>
    /// Gets a list of logs by trace id
    /// </summary>
    /// <param name="traceId">Trace ID</param>
    /// <returns>Returns a list of logs</returns>
    Task<List<Log>> GetLogsByTraceAsync(string traceId);
}
