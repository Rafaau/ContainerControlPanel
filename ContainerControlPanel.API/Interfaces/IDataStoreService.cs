using ContainerControlPanel.Domain.Models;

namespace ContainerControlPanel.API.Interfaces;

public interface IDataStoreService
{
    Task<List<SavedRequest>> GetRequestsAsync();

    Task SaveRequestAsync(SavedRequest request);

    Task DeleteRequestAsync(string id);

    Task PinRequestAsync(string id);

    Task SaveMetricsAsync(MetricsRoot metrics, string? serviceName = "", string? routeName = "");

    Task SaveTraceAsync(TracesRoot trace, string? traceId = "");

    Task SaveLogAsync(LogsRoot log, string? traceId = "");

    Task<List<TracesRoot>> GetTracesAsync(int timeOffset, string? resource, string? timestamp);

    Task<List<TracesRoot>> GetTraceAsync(string traceId);

    Task<List<MetricsRoot>> GetMetricsAsync();

    Task<List<LogsRoot>> GetLogsAsync(int timeOffset, string? timestamp, string? resource, int page, int pageSize);

    Task<LogsRoot> GetLogAsync(string traceId);
}
