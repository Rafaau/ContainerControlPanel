using ContainerControlPanel.Domain.Models;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using Refit;

namespace ContainerControlPanel.Web.Interfaces;

/// <summary>
/// Container API Refit interface.
/// </summary>
public interface IContainerAPI
{
    /// <summary>
    /// Gets the containers list.
    /// </summary>
    /// <param name="force">Indicates whether to force the refresh.</param>
    /// <param name="liveFilter">Indicates whether to apply live filter.</param>
    /// <returns>Returns the list of containers.</returns>
    [Get("/api/getContainersList?force={force}&liveFilter={liveFilter}")]
    Task<List<Container>> GetContainers(bool force, bool liveFilter);

    /// <summary>
    /// Stops a container.
    /// </summary>
    /// <param name="containerId">Container ID.</param>
    /// <returns>Returns the result.</returns>
    [Get("/api/stopContainer?containerId={containerId}")]
    Task<string> StopContainer(string containerId);

    /// <summary>
    /// Restarts a container.
    /// </summary>
    /// <param name="containerId">Container ID.</param>
    /// <returns>Returns the result.</returns>
    [Get("/api/restartContainer?containerId={containerId}")]
    Task RestartContainer(string containerId);

    /// <summary>
    /// Pauses a container.
    /// </summary>
    /// <param name="containerId">Container ID.</param>
    /// <returns>Returns the result.</returns>
    [Get("/api/startContainer?containerId={containerId}")]
    Task StartContainer(string containerId);

    /// <summary>
    /// Gets the container details.
    /// </summary>
    /// <param name="containerId">Container ID.</param>
    /// <returns>Returns the container details.</returns>
    [Get("/api/getContainerDetails?containerId={ContainerId}")]
    Task<ContainerDetails> GetContainerDetails(string containerId);

    /// <summary>
    /// Gets the container logs.
    /// </summary>
    /// <param name="containerId">Container ID.</param>
    /// <param name="timestamp">Timestamp.</param>
    /// <param name="date">Date.</param>
    /// <param name="timeFrom">Start time.</param>
    /// <param name="timeTo">End time.</param>
    /// <returns>Returns the container logs.</returns>
    [Get("/api/getContainerLogs?containerId={containerId}&timestamp={timestamp}&date={date}&timeFrom={timeFrom}&{timeTo}")]
    Task<string> GetContainerLogs(string containerId,
            string timestamp = null,
            DateTime? date = null,
            string timeFrom = "00:00:00",
            string timeTo = "23:59:59");

    /// <summary>
    /// Searches for compose files.
    /// </summary>
    /// <returns>Returns the list of compose files.</returns>
    [Get("/api/searchForComposes")]
    Task<List<ComposeFile>> SearchForComposes();

    /// <summary>
    /// Updates the compose file.
    /// </summary>
    /// <param name="filePath">Path to the file.</param>
    /// <param name="fileContent">File content.</param>
    /// <returns>Returns the result of the operation.</returns>
    [Put("/api/updateCompose?filePath={filePath}&content={fileContent}")]
    Task UpdateCompose(string filePath, string fileContent);

    /// <summary>
    /// Uploads the compose file.
    /// </summary>
    /// <param name="file">File to upload.</param>
    /// <returns>Returns the result of the operation.</returns>
    [Post("/api/uploadCompose")]
    Task UploadCompose([Body] MultipartFormDataContent file);

    /// <summary>
    /// Searches for images.
    /// </summary>
    /// <returns>Returns the list of images.</returns>
    [Get("/api/searchForImages")]
    Task<List<ImageFile>> SearchForImages();

    /// <summary>
    /// Uploads a single chunk of an image file.
    /// </summary>
    /// <param name="chunk">Chunk to upload.</param>
    /// <returns>Returns the result of the operation.</returns>
    [Post("/api/uploadChunk")]
    Task UploadChunk([Body] MultipartFormDataContent chunk);

    /// <summary>
    /// Merges the chunks of an image file.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <returns>Returns the result of the operation.</returns>
    [Post("/api/mergeChunks?fileName={fileName}")]
    Task MergeChunks(string fileName);
}
