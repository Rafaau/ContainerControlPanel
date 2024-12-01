using ContainerControlPanel.Domain.Models;
using ContainerControlPanel.Web.Components;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;

namespace ContainerControlPanel.Web.Pages;

public partial class Containers(HttpClient client)
{
    [Inject]
    IDialogService DialogService { get; set; }

    private HttpClient client { get; set; } = client;
    private List<Container> containers { get; set; } = new();
    private string liveFilter { get; set; } = "true";

    private bool _open;
    private Anchor _anchor;
    private string _width, _height;

    protected override async Task OnInitializedAsync()
    {
        await LoadContainers(false);
    }

    private async Task LoadContainers(bool force)
    {
        containers = await client.GetFromJsonAsync<List<Container>>($"api/getContainersList?force={force}&liveFilter={liveFilter}");
        this.StateHasChanged();
    }

    private async Task StopContainer(string containerId)
    {
        var container = containers.Find(x => x.ContainerId == containerId);
        container.Status = "Stopping...";

        string result = await client.GetStringAsync($"api/stopContainer?containerId={containerId}");

        if (result.Contains(container.ContainerId))
        {
            container.Status = "Exited";
        }
    }

    private async Task RestartContainer(string containerId)
    {
        var container = containers.Find(x => x.ContainerId == containerId);
        container.Status = "Restarting...";

        string result = await client.GetStringAsync($"api/restartContainer?containerId={containerId}");

        if (result.Contains(container.ContainerId))
        {
            container.Status = "Up 2 seconds";
        }
        else
        {
            container.Status = "Exited";
        }
    }

    private async Task StartContainer(string containerId)
    {
        var container = containers.Find(x => x.ContainerId == containerId);
        container.Status = "Starting...";
        string result = await client.GetStringAsync($"api/startContainer?containerId={containerId}");

        if (result.Contains(container.ContainerId))
        {
            container.Status = "Up 2 seconds";
        }
        else
        {
            container.Status = "Exited";
        }
    }

    private Task OpenDetailsDialogAsync(string containerId)
    {
        var options = new DialogOptions { CloseOnEscapeKey = true };

        return DialogService.ShowAsync<ContainerDetailsDialog>(
            "Simple Dialog", 
            new DialogParameters() 
            { 
                { "ContainerId", containerId } 
            }, 
            options
        );
    }
}