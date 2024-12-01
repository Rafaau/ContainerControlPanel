using System.Text.Json.Serialization;

namespace ContainerControlPanel.Domain.Models;

public class Container
{
    [JsonPropertyName("ID")]
    public string ContainerId { get; set; }
    public string Image { get; set; }
    public string Command { get; set; }
    public string CreatedAt { get; set; }
    public DateTime Created
    {
        get
        {
            return DateTime.Parse(CreatedAt.Substring(0, 19));
        }
    }
    public string Status { get; set; }
    public string Ports { get; set; }
    public string Names { get; set; }
}