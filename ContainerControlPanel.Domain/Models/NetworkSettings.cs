using System.Text.Json.Serialization;

namespace ContainerControlPanel.Domain.Models;

public class NetworkSettings
{
    public Dictionary<string, List<PortMapping>> Ports { get; set; }
}

public class PortMapping
{
    public string HostIp { get; set; }

    public string HostPort { get; set; }
}