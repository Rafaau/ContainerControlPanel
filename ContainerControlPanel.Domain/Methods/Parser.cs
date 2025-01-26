using ContainerControlPanel.Domain.Models;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;
using YamlDotNet.RepresentationModel;

namespace ContainerControlPanel.Domain.Methods;

/// <summary>
/// Class to parse the output of the Docker commands
/// </summary>
public static class Parser
{
    /// <summary>
    /// Parses the output of the Docker ps command
    /// </summary>
    /// <param name="config">Configuration settings</param>
    /// <param name="output">String output of the Docker ps command</param>
    /// <returns>Returns a list of <see cref="Container"/> objects</returns>
    public async static Task<List<Container>> ParseContainers(IConfiguration config, string output)
    {
        List<Container> containers = new List<Container>();
        var containersOutput = output.Split(new[] { '{' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var containerOutput in containersOutput)
        {
            string containerObj = $"{{{containerOutput}";
            Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(containerObj));
            Container container = await JsonSerializer.DeserializeAsync<Container>(stream);

            List<string> namesToExclude = config["ExcludeByName"].Split(';').ToList();
            List<string> imagesToExclude = config["ExcludeByImage"].Split(';').ToList();
            bool excluded = false;

            foreach (var name in namesToExclude.Where(x => !string.IsNullOrEmpty(x)))
            {
                if (container.Names.Contains(name))
                {
                    excluded = true;
                }
            }

            foreach (var image in imagesToExclude.Where(x => !string.IsNullOrEmpty(x)))
            {
                if (container.Image.Contains(image))
                {
                    excluded = true;
                }
            }

            if (!excluded)
                containers.Add(container);
        }

        return containers.OrderByDescending(c => c.Created).ToList();
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

    /// <summary>
    /// Gets the service names from a Docker Compose file
    /// </summary>
    /// <param name="filePath">Path to the Docker Compose file</param>
    /// <returns>Returns a list of service names</returns>
    public static List<string> GetServiceNames(string filePath)
    {
        var serviceNames = new List<string>();

        using (var reader = new StreamReader(filePath))
        {
            var yaml = new YamlStream();
            yaml.Load(reader);

            var rootNode = (YamlMappingNode)yaml.Documents[0].RootNode;

            if (rootNode.Children.ContainsKey(new YamlScalarNode("services")))
            {
                var servicesNode = (YamlMappingNode)rootNode.Children[new YamlScalarNode("services")];

                foreach (var service in servicesNode.Children)
                {
                    var serviceName = service.Key.ToString();
                    serviceNames.Add(serviceName);
                }
            }
        }

        return serviceNames;
    }

    /// <summary>
    /// Parses the output of the Docker images command
    /// </summary>
    /// <param name="output">Output of the Docker images command</param>
    /// <returns>Returns a list of <see cref="Image"/> objects</returns>
    public async static Task<List<Image>> ParseImages(string output)
    {
        List<Image> images = new List<Image>();

        var imagesOutput = output.Split(new[] { '{' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var imageOutput in imagesOutput)
        {
            string imageObj = $"{{{imageOutput}";
            Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(imageObj));
            Image image = await JsonSerializer.DeserializeAsync<Image>(stream);
            images.Add(image);
        }

        return images.OrderByDescending(i => i.CreatedAt).ToList();
    }
}