namespace ContainerControlPanel.Domain.Models;

public class EnvironmentVariable
{
    public string Name { get; set; }
    public string Value { get; set; }
    public bool Hide { get; set; } = true;
}
