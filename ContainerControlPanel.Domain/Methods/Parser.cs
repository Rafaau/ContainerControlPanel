using ContainerControlPanel.Domain.Models;
using System.Text;
using System.Text.Json;

namespace ContainerControlPanel.Domain.Methods;

/// <summary>
/// Class to parse the output of the Docker commands
/// </summary>
public static class Parser
{
    /// <summary>
    /// Parses the output of the Docker ps command
    /// </summary>
    /// <param name="output">String output of the Docker ps command</param>
    /// <returns>Returns a list of <see cref="Container"/> objects</returns>
    public async static Task<List<Container>> ParseContainers(string output)
    {
        List<Container> containers = new List<Container>();
        var containersOutput = output.Split(new[] { '{' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var containerOutput in containersOutput)
        {
            string containerObj = $"{{{containerOutput}";
            Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(containerObj));
            Container container = await JsonSerializer.DeserializeAsync<Container>(stream);

            if (container.Names.Contains("ccp-compose"))
            {
                continue;
            }

            containers.Add(container);
        }

        return containers.OrderByDescending(c => c.Status).ToList();
    }

    /// <summary>
    /// Parses the output of the Docker inspect command
    /// </summary>
    /// <param name="output">String output of the Docker inspect command</param>
    /// <returns>Returns a <see cref="ContainerDetails"/> object</returns>
    public static ContainerDetails ParseContainerDetails(string output)
    {
        output = output.Substring(1, output.Length - 3);
        ContainerDetails containerDetails = JsonSerializer.Deserialize<ContainerDetails>(output);
        return containerDetails;
    }
}