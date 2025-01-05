namespace ContainerControlPanel.Domain.Models;

/// <summary>
/// Class to represent the container inspect command output
/// </summary>
public class ContainerDetails
{
    /// <summary>
    /// Gets or sets the container ID
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets the container name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the container Config section
    /// </summary>
    public Config Config { get; set; }

    /// <summary>
    /// Gets or sets the container NetworkSettings section
    /// </summary>
    public NetworkSettings NetworkSettings { get; set; }
}

/// <summary>
/// Class to represent the container Config section
/// </summary>
public class Config
{
    /// <summary>
    /// Gets or sets the environment variables
    /// </summary>
    public List<string> Env { get; set; }

    /// <summary>
    /// Field to store the environment variables
    /// </summary>
    private List<EnvironmentVariable> environmentVariables = new();

    /// <summary>
    /// Gets or sets the environment variables
    /// </summary>
    public List<EnvironmentVariable> EnvironmentVariables
    {
        get
        {
            if (environmentVariables.Count > 0)
            {
                return environmentVariables;
            }

            return Env.Select(envVar =>
            {
                var parts = envVar.Split('=');
                return new EnvironmentVariable
                {
                    Name = parts[0],
                    Value = parts[1]
                };
            }).ToList();
        }
        set => environmentVariables = value;
    }
}

/// <summary>
/// Class to represent the container NetworkSettings section
/// </summary>
public class NetworkSettings
{
    /// <summary>
    /// Gets or sets the ports
    /// </summary>
    public Dictionary<string, List<PortMapping>> Ports { get; set; }
}

/// <summary>
/// Class to represent the container port mapping
/// </summary>
public class PortMapping
{
    /// <summary>
    /// Gets or sets the host IP
    /// </summary>
    public string HostIp { get; set; }

    /// <summary>
    /// Gets or sets the host port
    /// </summary>
    public string HostPort { get; set; }
}
