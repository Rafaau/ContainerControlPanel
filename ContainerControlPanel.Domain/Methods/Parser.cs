using ContainerControlPanel.Domain.Models;
using System.Text.Json;

namespace ContainerControlPanel.Domain.Methods;

public static class Parser
{
    public static List<Container> ParseContainers(string output)
    {
        List<Container> containers = new List<Container>();
        var containersOutput = output.Split(new[] { '{' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var containerOutput in containersOutput)
        {
            string containerObj = $"{{{containerOutput}";
            Container container = JsonSerializer.Deserialize<Container>(containerObj);
            containers.Add(container);
        }

        return containers.OrderByDescending(c => c.Status).ToList();
    }

    public static ContainerDetails ParseContainerDetails(string output)
    {
        output = output.Substring(1, output.Length - 3);
        ContainerDetails containerDetails = JsonSerializer.Deserialize<ContainerDetails>(output);
        return containerDetails;
    }
}