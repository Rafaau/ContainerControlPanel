using ContainerControlPanel.Domain.Models;
using Refit;

namespace ContainerControlPanel.Web.Interfaces;

public interface ITelemetryAPI
{
    [Get("/v1/getTraces")]
    Task<List<TracesRoot>> GetTraces();

    [Get("/v1/getMetrics")]
    Task<List<MetricsRoot>> GetMetrics();
}
