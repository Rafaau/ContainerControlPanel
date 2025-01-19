using System.Text.Json.Serialization;

namespace ContainerControlPanel.Domain.Models;

/// <summary>
/// Class to represent the docker images command output
/// </summary>
public class Image
{
    /// <summary>
    /// Gets or sets the repository
    /// </summary>
    public string Repository { get; set; }

    /// <summary>
    /// Gets or sets the tag
    /// </summary>
    public string Tag { get; set; }

    /// <summary>
    /// Gets or sets the image ID
    /// </summary>
    [JsonPropertyName("ID")]
    public string ImageId { get; set; }

    /// <summary>
    /// Gets or sets the created since
    /// </summary>
    [JsonPropertyName("CreatedSince")]
    public string Created { get; set; }

    /// <summary>
    /// Gets or sets the created at
    /// </summary>
    public string CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the size
    /// </summary>
    public string Size { get; set; }
}
