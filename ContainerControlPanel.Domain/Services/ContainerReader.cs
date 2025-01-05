using System.Diagnostics;
using ContainerControlPanel.Domain.Methods;
using ContainerControlPanel.Domain.Models;

namespace ContainerControlPanel.Domain.Services;

/// <summary>
/// Class to read information about containers
/// </summary>
public static class ContainerReader
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
}
