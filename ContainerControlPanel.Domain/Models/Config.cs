using System.Text.Json.Serialization;

namespace ContainerControlPanel.Domain.Models;

public class Config
{
    public List<string> Env { get; set; }

    private List<EnvironmentVariable> environmentVariables = new();
    public List<EnvironmentVariable> EnvironmentVariables
    {
        get
        {
            if (environmentVariables.Count > 0)
            {
                return environmentVariables;
            }

            return Env.Select(envVar =>
            {
                var parts = envVar.Split('=');
                return new EnvironmentVariable
                {
                    Name = parts[0],
                    Value = parts[1]
                };
            }).ToList();
        }
        set => environmentVariables = value;
    }
}
