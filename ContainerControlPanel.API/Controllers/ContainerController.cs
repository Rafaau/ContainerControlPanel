using ContainerControlPanel.Domain.Models;
using ContainerControlPanel.Domain.Services;
using Microsoft.AspNetCore.Components.Forms;
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
        /// Configuration settings 
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContainerController"/> class.
        /// </summary>
        /// <param name="configuration">Configuration settings</param>
        public ContainerController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

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

        /// <summary>
        /// Gets the Docker compose files in the specified directory
        /// </summary>
        /// <returns>Returns the Docker compose files</returns>
        [HttpGet("SearchForComposes")]
        public async Task<IActionResult> SearchForComposes()
        {
            var composeFiles = await FileManager.SearchComposeFiles(_configuration["ComposeDir"]);
            return Ok(composeFiles);
        }

        /// <summary>
        /// Updates the content of a Docker compose file
        /// </summary>
        /// <param name="filePath">Path to the compose file</param>
        /// <param name="content">File content</param>
        /// <returns>Returns the result of the operation</returns>
        [HttpPut("UpdateCompose")]
        public async Task<IActionResult> UpdateCompose(string filePath, string content)
        {
            await FileManager.WriteFileContent(filePath, content);
            return Ok();
        }

        /// <summary>
        /// Uploads a Docker compose file to the specified directory
        /// </summary>
        /// <param name="file">IFormFile object</param>
        /// <returns>Returns the result of the operation</returns>
        [HttpPost("UploadCompose")]
        public async Task<IActionResult> UploadCompose([FromForm] IFormFile file)
        {
            await FileManager.UploadFile(_configuration["ComposeDir"], file);
            return Ok();
        }

        /// <summary>
        /// Searches for image files in the specified directory
        /// </summary>
        /// <returns>Returns the image files</returns>
        [HttpGet("SearchForImages")]
        public async Task<IActionResult> SearchForImages()
        {
            var imageFiles = await FileManager.SearchImageFiles(_configuration["ImagesDir"]);
            return Ok(imageFiles);
        }

        /// <summary>
        /// Uploads a single chunk of an image file
        /// </summary>
        /// <param name="chunk">IFormFile object</param>
        /// <returns>Returns the result of the operation</returns>
        [HttpPost("UploadChunk")]
        public async Task<IActionResult> UploadChunk([FromForm] IFormFile chunk)
        {
            await FileManager.UploadFile(_configuration["ImagesDir"], chunk);
            return Ok();
        }

        /// <summary>
        /// Merges the chunks of an image file into a single file
        /// </summary>
        /// <param name="fileName">Name of the file</param>
        /// <returns>Returns the result of the operation</returns>
        [HttpPost("MergeChunks")]
        public async Task<IActionResult> MergeChunks(string fileName)
        {
            await FileManager.MergeChunks(_configuration["ImagesDir"], fileName);
            return Ok();
        }
    }
}
