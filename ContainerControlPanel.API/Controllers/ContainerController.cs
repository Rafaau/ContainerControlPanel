using ContainerControlPanel.Domain.Models;
using ContainerControlPanel.Domain.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Caching.Memory;

namespace ContainerControlPanel.API.Controllers
{
    [ApiController]
    [Route("/api")]
    public class ContainerController : ControllerBase
    {
        [HttpGet("GetContainersList")]
        public async Task<IActionResult> GetContainers(IMemoryCache memoryCache, bool force = false, bool liveFilter = false)
        {
            var cachedResponse = memoryCache.Get<List<Container>>("containers");

            if (cachedResponse != null && !force)
            {
                return Ok(cachedResponse);
            }

            var response = await ContainerReader.GetContainersListAsync(liveFilter);

            memoryCache.Set("containers", response, TimeSpan.FromMinutes(1));

            return Ok(response);
        }

        [HttpGet("GetContainerLogs")]
        public async Task<IActionResult> GetContainerLogs(string containerId, string timestamp = null, DateTime? date = null)
        {
            var result = await ContainerReader.GetContainerLogsAsync(containerId, timestamp, date);
            return Ok(result);
        }

        [HttpGet("RestartContainer")]
        public async Task<IActionResult> RestartContainer(string containerId)
        {
            var result = await ContainerReader.RestartContainerAsync(containerId);
            return Ok(result);
        }

        [HttpGet("StopContainer")]
        public async Task<IActionResult> StopContainer(string containerId)
        {
            var result = await ContainerReader.StopContainerAsync(containerId);
            return Ok(result);
        }

        [HttpGet("StartContainer")]
        public async Task<IActionResult> StartContainer(string containerId)
        {
            var result = await ContainerReader.StartContainerAsync(containerId);
            return Ok(result);
        }

        [HttpGet("GetContainerDetails")]
        public async Task<IActionResult> GetContainerDetails(string containerId)
        {
            var result = await ContainerReader.GetContainerDetailsAsync(containerId);
            return Ok(result);
        }
    }
}
