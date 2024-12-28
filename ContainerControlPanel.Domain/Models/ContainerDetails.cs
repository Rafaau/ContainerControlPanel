namespace ContainerControlPanel.Domain.Models;

public class ContainerDetails
{
    public string Id { get; set; }
    public string Name { get; set; }
    public Config Config { get; set; }
    public NetworkSettings NetworkSettings { get; set; }
}
