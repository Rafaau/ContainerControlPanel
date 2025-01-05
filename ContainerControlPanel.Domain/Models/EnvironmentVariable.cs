namespace ContainerControlPanel.Domain.Models;

/// <summary>
/// Class to represent an environment variable
/// </summary>
public class EnvironmentVariable
{
    /// <summary>
    /// Gets or sets the name of the environment variable
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the value of the environment variable
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the environment variable should be hidden
    /// </summary>
    public bool Hide { get; set; } = true;
}
