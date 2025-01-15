namespace ContainerControlPanel.Domain.Models;

/// <summary>
/// Compose file model.
/// </summary>
public class ComposeFile
{
    /// <summary>
    /// Path to the Docker Compose file.
    /// </summary>
    public string FilePath { get; set; }

    /// <summary>
    /// Name of the Docker Compose file.
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    /// Content of the Docker Compose file.
    /// </summary>
    public string FileContent { get; set; }

    /// <summary>
    /// List of service names in the Docker Compose file.
    /// </summary>
    public List<string> ServiceNames { get; set; }
}

/// <summary>
/// Extension methods for the <see cref="ComposeFile"/> class.
/// </summary>
public static class ComposeFileExtensions
{
    /// <summary>
    /// Tries to get the Docker Compose file for a service.
    /// </summary>
    /// <param name="serviceName">Name of the service</param>
    /// <param name="composeFiles">List of Docker Compose files</param>
    /// <returns>Returns the Docker Compose file for the specified service</returns>
    public static ComposeFile? TryGetComposeFile(this string serviceName, List<ComposeFile> composeFiles)
    {
        foreach (var composeFile in composeFiles)
        {
            foreach (var service in composeFile.ServiceNames)
            {
                if (service.Contains("redis")
                    || service.Contains("postgres"))
                {
                    continue;
                }

                if (serviceName.Contains(service))
                {
                    return composeFile;
                }
            }
        }

        return null;
    }
}
