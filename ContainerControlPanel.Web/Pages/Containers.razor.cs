using ContainerControlPanel.Domain.Models;
using ContainerControlPanel.Web.Components;
using ContainerControlPanel.Web.Enums;
using ContainerControlPanel.Web.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace ContainerControlPanel.Web.Pages;

public partial class Containers(IContainerAPI containerAPI) : IAsyncDisposable
{
    [Inject]
    IServiceProvider ServiceProvider { get; set; }

    [Inject]
    IDialogService DialogService { get; set; }

    [Inject]
    IConfiguration Configuration { get; set; }

    [Inject]
    NavigationManager NavigationManager { get; set; }

    [Inject]
    IStringLocalizer<Locales.Resource> Localizer { get; set; }

    [Inject]
    IMemoryCache MemoryCache { get; set; }

    [Parameter]
    public string? LiveFilter { get; set; }

    private bool liveFilter
    {
        get => bool.Parse(LiveFilter ?? "false");
        set
        {
            LiveFilter = value.ToString();
            NavigationManager.NavigateTo($"/{value}");
        }
    }

    private IContainerAPI containerAPI { get; set; } = containerAPI;
    private List<Container> containers { get; set; } = new();

    private bool _open;
    private Anchor _anchor;
    private string _width, _height;

    protected override async Task OnInitializedAsync()
    {
        LiveFilter ??= "true";
        await LoadContainers(false);

        if (bool.Parse(Configuration["Realtime"]))
        {
            _ = Task.Run(async () =>
            {
                while (true)
                {
                    await LoadContainers(true);
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            });
        }   
    }

    private async Task LoadContainers(bool force)
    {
        if (MemoryCache.TryGetValue("containers", out List<Container> cachedContainers) && !force)
        {
            containers = cachedContainers;
            this.StateHasChanged();

            var result = await containerAPI.GetContainers(force, liveFilter);

            if (result.Count != containers.Count)
            {
                MemoryCache.Set("containers", result);
                containers = result;
                this.StateHasChanged();
            }
        }
        else
        {
            var result = await containerAPI.GetContainers(force, liveFilter);
            MemoryCache.Set("containers", result);
            containers = result;
            this.StateHasChanged();
        }
    }

    private async Task StopContainer(string containerId)
    {
        var container = containers.Find(x => x.ContainerId == containerId);
        container.Status = "Stopping...";

        string result = await containerAPI.StopContainer(containerId);

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
            await containerAPI.RestartContainer(containerId);
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
            await containerAPI.StartContainer(containerId);
        }
    }

    private Task OpenDetailsDialogAsync(string containerId)
    {
        var options = new DialogOptions { CloseOnEscapeKey = true };

        return DialogService.ShowAsync<ContainerDetailsDialog>(
            "", 
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
            "",
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

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}