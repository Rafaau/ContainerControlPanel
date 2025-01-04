using ContainerControlPanel.Domain.Models;
using ContainerControlPanel.Domain.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace ContainerControlPanel.API.Controllers
{
    /// <summary>
    /// Controller for managing Docker containers
    /// </summary>
    [ApiController]
    [Route("/api")]
    public class ContainerController : ControllerBase
    {
        /// <summary>
        /// Get a list of containers
        /// </summary>
        /// <param name="memoryCache">Memory cache</param>
        /// <param name="force">Indicates whether to force a cache refresh</param>
        /// <param name="liveFilter">Indicates whether to filter running containers only</param>
        /// <returns>Returns a list of containers</returns>
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

        /// <summary>
        /// Get logs for a Docker container
        /// </summary>
        /// <param name="containerId">Container ID</param>
        /// <param name="timestamp">Range start timestamp</param>
        /// <param name="date">Specific date</param>
        /// <param name="timeFrom">Time range start</param>
        /// <param name="timeTo">Time range end</param>
        /// <returns>Returns the logs for the specified container</returns>
        [HttpGet("GetContainerLogs")]
        public async Task<IActionResult> GetContainerLogs(
            string containerId, 
            string timestamp = null, 
            DateTime? date = null,
            string timeFrom = "00:00:00",
            string timeTo = "23:59:59")
        {
            var result = await ContainerReader.GetContainerLogsAsync(containerId, timestamp, date, timeFrom, timeTo);
            return Ok(result);
        }

        /// <summary>
        /// Restart a Docker container
        /// </summary>
        /// <param name="containerId">Container ID</param>
        /// <returns>Returns the result of the operation</returns>
        [HttpGet("RestartContainer")]
        public async Task<IActionResult> RestartContainer(string containerId)
        {
            var result = await ContainerReader.RestartContainerAsync(containerId);
            return Ok(result);
        }

        /// <summary>
        /// Stop a Docker container
        /// </summary>
        /// <param name="containerId">Container ID</param>
        /// <returns>Returns the result of the operation</returns>
        [HttpGet("StopContainer")]
        public async Task<IActionResult> StopContainer(string containerId)
        {
            var result = await ContainerReader.StopContainerAsync(containerId);
            return Ok(result);
        }

        /// <summary>
        /// Start a Docker container
        /// </summary>
        /// <param name="containerId">Container ID</param>
        /// <returns>Returns the result of the operation</returns>
        [HttpGet("StartContainer")]
        public async Task<IActionResult> StartContainer(string containerId)
        {
            var result = await ContainerReader.StartContainerAsync(containerId);
            return Ok(result);
        }

        /// <summary>
        /// Get details for a Docker container
        /// </summary>
        /// <param name="containerId">Container ID</param>
        /// <returns>Returns the details for the specified container</returns>
        [HttpGet("GetContainerDetails")]
        public async Task<IActionResult> GetContainerDetails(string containerId)
        {
            var result = await ContainerReader.GetContainerDetailsAsync(containerId);
            return Ok(result);
        }
    }
}
