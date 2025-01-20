namespace ContainerControlPanel.Domain.Models;

/// <summary>
/// Image file model.
/// </summary>
public class ImageFile
{
    /// <summary>
    /// Gets or sets the path to the image file.
    /// </summary>
    public string FilePath { get; set; }

    /// <summary>
    /// Gets or sets the name of the image file.
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    /// Gets or sets the size of the image file.
    /// </summary>
    public string Size { get; set; }
}
