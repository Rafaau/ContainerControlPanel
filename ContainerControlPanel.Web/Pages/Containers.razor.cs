using ContainerControlPanel.Domain.Models;
using ContainerControlPanel.Web.Components;
using ContainerControlPanel.Web.Enums;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;

namespace ContainerControlPanel.Web.Pages;

public partial class Containers(HttpClient client)
{
    [Inject]
    IServiceProvider ServiceProvider { get; set; }

    [Inject]
    IDialogService DialogService { get; set; }

    [Inject]
    IConfiguration Configuration { get; set; }
    [Inject]
    NavigationManager NavigationManager { get; set; }

    private HttpClient client { get; set; } = client;
    private List<Container> containers { get; set; } = new();
    private bool liveFilter { get; set; } = true;

    private bool _open;
    private Anchor _anchor;
    private string _width, _height;

    protected override async Task OnInitializedAsync()
    {
        await LoadContainers(false);

        if (bool.Parse(Configuration["Realtime"]))
        {
            _ = Task.Run(async () =>
            {
                while (true)
                {
                    await LoadContainers(true);
                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
            });
        }   
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
        
        var dialog = await OpenStartContainerDialogAsync(containerId, ActionType.Restart);
        var option = await dialog.Result;

        if (option.Data.GetType() != typeof(StartOption))
            return;
        
        if ((StartOption)option.Data == StartOption.JustStart)
        {
            container.Status = "Restarting...";
            this.StateHasChanged();
            await client.GetStringAsync($"api/restartContainer?containerId={containerId}");
        }
    }

    private async Task StartContainer(string containerId)
    {
        var container = containers.Find(x => x.ContainerId == containerId);  

        var dialog = await OpenStartContainerDialogAsync(containerId, ActionType.Start);
        var option = await dialog.Result;

        if (option.Data.GetType() != typeof(StartOption))
            return;

        if ((StartOption)option.Data == StartOption.JustStart)
        {
            container.Status = "Starting...";
            this.StateHasChanged();
            await client.GetStringAsync($"api/startContainer?containerId={containerId}");
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

    private async Task<IDialogReference> OpenStartContainerDialogAsync(string containerId, ActionType actionType)
    {
        var options = new DialogOptions { CloseOnEscapeKey = true };
        return await DialogService.ShowAsync<StartContainerDialog>(
            "Simple Dialog",
            new DialogParameters()
            {
                { "ContainerId", containerId },
                { "ActionType", actionType }
            },
            options
        );
    }

    private void ViewLogs(string containerId)
    {
        NavigationManager.NavigateTo($"/logs/{containerId}");
    }
}