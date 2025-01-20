using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace ContainerControlPanel.Domain.Models;

/// <summary>
/// Class to represent the docker ps command output
/// </summary>
public class Container
{
    /// <summary>
    /// Gets or sets the container ID
    /// </summary>
    [JsonPropertyName("ID")]
    public string ContainerId { get; set; }

    /// <summary>
    /// Gets or sets the image name
    /// </summary>
    public string Image { get; set; }

    /// <summary>
    /// Gets or sets the command
    /// </summary>
    public string Command { get; set; }

    /// <summary>
    /// Gets or sets the created at date
    /// </summary>
    public string CreatedAt { get; set; }

    /// <summary>
    /// Gets the created at date as a <see cref="DateTime"/> object
    /// </summary>
    public DateTime Created
    {
        get
        {
            return DateTime.Parse(CreatedAt.Substring(0, 19));
        }
    }

    /// <summary>
    /// Gets or sets the status
    /// </summary>
    public string Status { get; set; }

    /// <summary>
    /// Gets or sets the ports
    /// </summary>
    public string Ports { get; set; }

    /// <summary>
    /// Gets or sets the names
    /// </summary>
    public string Names { get; set; }

    /// <summary>
    /// Gets or sets the labels
    /// </summary>
    public string Labels { get; set; }
}