using ContainerControlPanel.Domain.Models;
using Refit;

namespace ContainerControlPanel.Web.Interfaces;

public interface ITelemetryAPI
{
    [Get("/v1/getTraces")]
    Task<List<Root>> GetTraces();
}
