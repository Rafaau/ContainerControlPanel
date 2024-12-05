using ContainerControlPanel.Domain.Models;
using Refit;

namespace ContainerControlPanel.Web.Interfaces;

public interface IContainerAPI
{
    [Get("/api/getContainersList?force={force}&liveFilter={liveFilter}")]
    Task<List<Container>> GetContainers(bool force, bool liveFilter);

    [Get("/api/stopContainer?containerId={containerId}")]
    Task<string> StopContainer(string containerId);

    [Get("/api/restartContainer?containerId={containerId}")]
    Task RestartContainer(string containerId);

    [Get("/api/startContainer?containerId={containerId}")]
    Task StartContainer(string containerId);

    [Get("/api/getContainerDetails?containerId={ContainerId}")]
    Task<ContainerDetails> GetContainerDetails(string containerId);

    [Get("/api/getContainerLogs?containerId={containerId}&timestamp={timestamp}&date={date}&timeFrom={timeFrom}&{timeTo}")]
    Task<string> GetContainerLogs(string containerId,
            string timestamp = null,
            DateTime? date = null,
            string timeFrom = "00:00:00",
            string timeTo = "23:59:59");
}
