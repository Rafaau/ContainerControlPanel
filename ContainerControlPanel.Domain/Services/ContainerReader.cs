using System.Diagnostics;
using System.Globalization;
using ContainerControlPanel.Domain.Methods;
using ContainerControlPanel.Domain.Models;

namespace ContainerControlPanel.Domain.Services;

public static class ContainerReader
{
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
