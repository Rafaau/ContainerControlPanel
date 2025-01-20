using System.Diagnostics;
using ContainerControlPanel.Domain.Methods;
using ContainerControlPanel.Domain.Models;

namespace ContainerControlPanel.Domain.Services;

/// <summary>
/// Class to read information about containers
/// </summary>
public static class ContainerManager
{
    /// <summary>
    /// Gets a list of containers
    /// </summary>
    /// <param name="liveFilter">Indicates whether to filter live containers</param>
    /// <returns>Returns a list of <see cref="Container"/> objects</returns>
    public static async Task<List<Container>> GetContainersListAsync(bool liveFilter = false)
    {
        string argString = liveFilter 
            ? @"ps --format=""{{json .}}"""
            : @"ps -a --format=""{{json .}}""";

        ProcessStartInfo _containersListProcessStartInfo = new ProcessStartInfo
        {
            FileName = "docker",
            Arguments = argString,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (Process process = Process.Start(_containersListProcessStartInfo))
        {
            using (StreamReader reader = process.StandardOutput)
            {
                string output = await reader.ReadToEndAsync();
                return await Parser.ParseContainers(output);
            }
        }
    }

    /// <summary>
    /// Gets the logs of a container
    /// </summary>
    /// <param name="containerId">Container ID</param>
    /// <param name="timestamp">Timestamp</param>
    /// <param name="date">Date</param>
    /// <param name="timeFrom">Start time</param>
    /// <param name="timeTo">End time</param>
    /// <returns>Returns the logs of the container</returns>
    public static async Task<string> GetContainerLogsAsync(
        string containerId, 
        string timestamp = null, 
        DateTime? date = null,
        string timeFrom = "00:00:00",
        string timeTo = "23:59:59")
    {
        string filterString = string.Empty;

        if (date.HasValue) 
        {
            filterString += $" --since \"{date.Value.ToString($"yyyy-MM-ddT{timeFrom}")}\"";
            filterString += $" --until \"{date.Value.ToString($"yyyy-MM-ddT{timeTo}")}\"";
        }
        else if (timestamp != null)
        {
            filterString = $"--since {timestamp}";
        }

        ProcessStartInfo _containerLogsProcessStartInfo = new ProcessStartInfo
        {
            FileName = "docker",
            Arguments = $"logs {containerId} {filterString}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (Process process = Process.Start(_containerLogsProcessStartInfo))
        {
            using (StreamReader reader = process.StandardOutput)
            {
                return await reader.ReadToEndAsync();      
            }
        }
    }

    /// <summary>
    /// Stops a container
    /// </summary>
    /// <param name="containerId">Container ID</param>
    /// <returns>Returns the output of the command</returns>
    public static async Task<string> StopContainerAsync(string containerId)
    {
        ProcessStartInfo _stopContainerProcessStartInfo = new ProcessStartInfo
        {
            FileName = "docker",
            Arguments = $"stop {containerId}",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        using (Process process = Process.Start(_stopContainerProcessStartInfo))
        {
            using (StreamReader reader = process.StandardOutput)
            {
                return await reader.ReadToEndAsync();
            }
        }
    }

    /// <summary>
    /// Restarts a container
    /// </summary>
    /// <param name="containerId">Container ID</param>
    /// <returns>Returns the output of the command</returns>
    public static async Task<string> RestartContainerAsync(string containerId)
    {
        ProcessStartInfo _restartContainerProcessStartInfo = new ProcessStartInfo
        {
            FileName = "docker",
            Arguments = $"restart {containerId}",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (Process process = Process.Start(_restartContainerProcessStartInfo))
        {
            using (StreamReader reader = process.StandardOutput)
            {
                return await reader.ReadToEndAsync();
            }
        }
    }

    /// <summary>
    /// Starts a container
    /// </summary>
    /// <param name="containerId">Container ID</param>
    /// <returns>Returns the output of the command</returns>
    public static async Task<string> StartContainerAsync(string containerId)
    {
        ProcessStartInfo _startContainerProcessStartInfo = new ProcessStartInfo
        {
            FileName = "docker",
            Arguments = $"start {containerId}",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        using (Process process = Process.Start(_startContainerProcessStartInfo))
        {
            using (StreamReader reader = process.StandardOutput)
            {
                return await reader.ReadToEndAsync();
            }
        }
    }

    /// <summary>
    /// Gets the details of a container
    /// </summary>
    /// <param name="containerId">Container ID</param>
    /// <returns>Returns a <see cref="ContainerDetails"/> object</returns>
    public static async Task<ContainerDetails> GetContainerDetailsAsync(string containerId)
    {
        ProcessStartInfo _containerEnvironmentVariablesProcessStartInfo = new ProcessStartInfo
        {
            FileName = "docker",
            Arguments = $"inspect {containerId}",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        using (Process process = Process.Start(_containerEnvironmentVariablesProcessStartInfo))
        {
            using (StreamReader reader = process.StandardOutput)
            {
                string output = await reader.ReadToEndAsync();
                return Parser.ParseContainerDetails(output);
            }
        }
    }

    /// <summary>
    /// Gets the currently available Docker images
    /// </summary>
    /// <returns>Returns a list of <see cref="Image"/> objects</returns>
    public static async Task<List<Image>> GetImagesAsync()
    {
        ProcessStartInfo _imagesProcessStartInfo = new ProcessStartInfo
        {
            FileName = "docker",
            Arguments = "images --format=\"{{json .}}\"",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (Process process = Process.Start(_imagesProcessStartInfo))
        {
            using (StreamReader reader = process.StandardOutput)
            {
                string output = await reader.ReadToEndAsync();
                return await Parser.ParseImages(output);
            }
        }
    }

    /// <summary>
    /// Removes a Docker image by ID
    /// </summary>
    /// <param name="imageId">Image ID</param>
    /// <returns>Returns a boolean indicating whether the image was removed</returns>
    public static async Task<bool> RemoveImageAsync(string imageId)
    {
        ProcessStartInfo _removeImageProcessStartInfo = new ProcessStartInfo
        {
            FileName = "docker",
            Arguments = $"rmi -f {imageId}",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        using (Process process = Process.Start(_removeImageProcessStartInfo))
        {
            using (StreamReader reader = process.StandardOutput)
            {
                string output = await reader.ReadToEndAsync();
                return output.Contains("Deleted");
            }
        }
    }

    /// <summary>
    /// Executes a shell command
    /// </summary>
    /// <param name="command">Command string</param>
    /// <returns>Returns a boolean indicating whether the command was executed successfully</returns>
    public static async Task<bool> ExecuteCommand(string command)
    {
        ProcessStartInfo _executeCommandProcessStartInfo = new ProcessStartInfo
        {
            FileName = "docker",
            Arguments = command,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        try
        {
            using (Process process = Process.Start(_executeCommandProcessStartInfo))
            {
                using (StreamReader reader = process.StandardOutput)
                {
                    string output = await reader.ReadToEndAsync();
                    return command.Contains("load") && output.Contains("Loaded")
                        || command.Contains("compose") && output.Contains("Started")
                        || command.Contains("run") && !output.Contains("Error");
                }
            }
        }
        catch (Exception)
        {
            return false;
        }
    }
}
