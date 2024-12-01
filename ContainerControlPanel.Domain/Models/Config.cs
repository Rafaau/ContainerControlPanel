using System.Text.Json.Serialization;

namespace ContainerControlPanel.Domain.Models;

public class Config
{
    [JsonPropertyName("Env")]
    public List<string> EnvironmentVariables { get; set; }
}
