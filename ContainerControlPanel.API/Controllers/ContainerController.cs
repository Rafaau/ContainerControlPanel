using ContainerControlPanel.API.Authorization;
using ContainerControlPanel.API.Interfaces;
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
    [TokenAuthorize]
    public class ContainerController : ControllerBase
    {
        /// <summary>
        /// Configuration settings 
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Redis service 
        /// </summary>
        private readonly IDataStoreService _dataStoreService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContainerController"/> class.
        /// </summary>
        /// <param name="configuration">Configuration settings</param>
        /// <param name="redisService">Redis service</param>
        public ContainerController(IConfiguration configuration, IDataStoreService dataStoreService)
        {
            _configuration = configuration;
            _dataStoreService = dataStoreService;
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

            var response = await ContainerManager.GetContainersListAsync(_configuration, liveFilter);

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
            var result = await ContainerManager.GetContainerLogsAsync(containerId, timestamp, date, timeFrom, timeTo);
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
            var result = await ContainerManager.RestartContainerAsync(containerId);
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
            var result = await ContainerManager.StopContainerAsync(containerId);
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
            var result = await ContainerManager.StartContainerAsync(containerId);
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
            var result = await ContainerManager.GetContainerDetailsAsync(containerId);
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
            Console.WriteLine("Uploading chunk" + chunk.FileName);
            await FileManager.UploadChunk(_configuration["ImagesDir"], chunk);
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

        /// <summary>
        /// Gets the currently available Docker images
        /// </summary>
        /// <returns>Returns the list of images</returns>
        [HttpGet("GetImages")]
        public async Task<IActionResult> GetImages()
        {
            var images = await ContainerManager.GetImagesAsync();
            return Ok(images);
        }

        /// <summary>
        /// Removes a Docker image by ID
        /// </summary>
        /// <param name="imageId">Image ID</param>
        /// <returns>Returns the result of the operation</returns>
        [HttpDelete("RemoveImage")]
        public async Task<IActionResult> RemoveImage(string imageId)
        {
            var result = await ContainerManager.RemoveImageAsync(imageId);
            return Ok(result);
        }

        /// <summary>
        /// Gets the currently available Docker volumes
        /// </summary>
        /// <returns>Returns the list of volumes</returns>
        [HttpGet("GetVolumes")]
        public async Task<IActionResult> GetVolumes()
        {
            var volumes = await ContainerManager.GetVolumesAsync();
            return Ok(volumes);
        }

        /// <summary>
        /// Removes a Docker volume by ID
        /// </summary>
        /// <param name="volumeId">Volume ID</param>
        /// <returns>Returns the result of the operation</returns>
        [HttpDelete("RemoveVolume")]
        public async Task<IActionResult> RemoveVolume(string volumeId)
        {
            var result = await ContainerManager.RemoveVolumeAsync(volumeId);
            return Ok(result);
        }

        /// <summary>
        /// Executes a shell command
        /// </summary>
        /// <param name="command">Command string</param>
        /// <returns>Returns the result of the operation</returns>
        [HttpPost("ExecuteCommand")]
        public async Task<IActionResult> ExecuteCommand(string command)
        {
            var result = await ContainerManager.ExecuteCommand(command);
            return Ok(result);
        }

        /// <summary>
        /// Saves a request to the cache
        /// </summary>
        /// <param name="request">Request object</param>
        /// <returns>Returns the result of the operation</returns>
        [HttpPost("SaveRequest")]
        public async Task<IActionResult> SaveRequest([FromBody] SavedRequest request)
        {
            try
            {
                await _dataStoreService.SaveRequestAsync(request);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Gets the saved requests from the cache
        /// </summary>
        /// <returns>Returns the list of saved requests</returns>
        [HttpGet("GetSavedRequests")]
        public async Task<IActionResult> GetSavedRequests()
        {
            try
            {
                List<SavedRequest> requests = await _dataStoreService.GetRequestsAsync();

                return Ok(requests);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Removes a request from the cache
        /// </summary>
        /// <param name="requestId">ID of the request</param>
        /// <returns>Returns the result of the operation</returns>
        [HttpDelete("RemoveRequest")]
        public async Task<IActionResult> RemoveRequest(string requestId)
        {
            try
            {
                await _dataStoreService.DeleteRequestAsync(requestId);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Sets a request as pinned or unpinned
        /// </summary>
        /// <param name="requestId">ID of the request</param>
        /// <returns>Returns the result of the operation</returns>
        [HttpPatch("PinRequest")]
        public async Task<IActionResult> PinRequest(string requestId)
        {
            try
            {
                await _dataStoreService.PinRequestAsync(requestId);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Gets the MagicInput from data store
        /// </summary>
        /// <returns>Returns the MagicInput</returns>
        [HttpGet("GetMagicInput")]
        public async Task<IActionResult> GetMagicInput()
        {
            try
            {
                var result = await _dataStoreService.GetMagicInput();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Saves the MagicInput to data store
        /// </summary>
        /// <param name="magicInput">MagicInput string</param>
        /// <returns>Returns the result of the operation</returns>
        [HttpPost("SaveMagicInput")]
        public async Task<IActionResult> SaveMagicInput(string magicInput)
        {
            try
            {
                await _dataStoreService.SaveMagicInput(magicInput);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
