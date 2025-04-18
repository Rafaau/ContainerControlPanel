namespace ContainerControlPanel.Domain.Models;

/// <summary>
/// Represents a Docker volume.
/// </summary>
public class Volume
{
    /// <summary>
    /// Gets or sets the volume ID.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the volume driver.
    /// </summary>
    public string Driver { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the volume labels.
    /// </summary>
    public string Labels { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the volume mountpoint.
    /// </summary>
    public string Mountpoint { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the volume links.
    /// </summary>
    public string Links { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the volume scope.
    /// </summary>
    public string Scope { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the volume size.
    /// </summary>
    public string Size { get; set; } = string.Empty;
}
