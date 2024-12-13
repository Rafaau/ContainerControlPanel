using ContainerControlPanel.Domain.Models;
using Refit;

namespace ContainerControlPanel.Web.Interfaces;

public interface ITelemetryAPI
{
    [Get("/v1/getTraces?resource={resource}")]
    Task<List<TracesRoot>> GetTraces(string? resource = null);

    [Get("/v1/getMetrics")]
    Task<List<MetricsRoot>> GetMetrics();
}
