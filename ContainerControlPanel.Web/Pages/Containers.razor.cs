using ContainerControlPanel.Domain.Models;
using ContainerControlPanel.Web.Components;
using ContainerControlPanel.Web.Enums;
using ContainerControlPanel.Web.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace ContainerControlPanel.Web.Pages;

public partial class Containers(IContainerAPI containerAPI) : IDisposable
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
            NavigationManager.NavigateTo($"/containers/{value}");
            MemoryCache.Set("lastContainersHref", $"/containers/{value}");
        }
    }

    private IContainerAPI containerAPI { get; set; } = containerAPI;
    private List<Container> containers { get; set; } = new();

    private bool _open;
    private Anchor _anchor;
    private string _width, _height;

    private readonly CancellationTokenSource _cts = new();

    protected override async Task OnInitializedAsync()
    {
        if (MemoryCache.TryGetValue("lastContainersHref", out string cachedHref))
        {
            NavigationManager.NavigateTo(cachedHref);
        }
        else
        {
            LiveFilter ??= "true";
        }

        await LoadContainers(true);

        if (bool.Parse(Configuration["Realtime"]))
        {
            _ = Task.Run(async () =>
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        await LoadContainers(true);
                        await Task.Delay(TimeSpan.FromSeconds(1), _cts.Token);
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }
                }
            });
        }   
    }

    private async Task LoadContainers(bool force)
    {
        if (containers.Count() == 0
            && MemoryCache.TryGetValue("containers", out List<Container> cachedContainers))
        {
            containers = cachedContainers;
            this.StateHasChanged();
        }
        else
        {
            var result = await containerAPI.GetContainers(force, liveFilter);
            MemoryCache.Set("containers", result);
            containers = result;
            this.StateHasChanged();
        }

        foreach (var container in containers)
        {
            if (container.Status.Contains("Up") && !MemoryCache.TryGetValue($"containerDetails_{container.ContainerId}", out _))
            {
                ContainerDetails containerDetails = await containerAPI.GetContainerDetails(container.ContainerId);

                MemoryCache.Set($"containerDetails_{container.ContainerId}", containerDetails);
            }
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
        ContainerDetails containerDetails = null;

        if (MemoryCache.TryGetValue($"containerDetails_{containerId}", out ContainerDetails _containerDetails))
        {
            containerDetails = _containerDetails;
        }

        var options = new DialogOptions { CloseOnEscapeKey = true };

        return DialogService.ShowAsync<ContainerDetailsDialog>(
            "", 
            new DialogParameters() 
            { 
                { "ContainerId", containerId },
                { "ContainerDetails", containerDetails }
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

    private bool IsASPNETCoreContainer(string containerId)
    {
        if (MemoryCache.TryGetValue($"containerDetails_{containerId}", out ContainerDetails containerDetails))
        {
            return containerDetails.Config.EnvironmentVariables.Any(x => x.Name.Contains("ASPNETCORE"));
        }
        else
        {
            return false;
        }
    }

    private void ViewLogs(string containerId)
    {
        NavigationManager.NavigateTo($"/logs/{containerId}");
    }

    private void ViewApiDocs(string containerId)
    {
        NavigationManager.NavigateTo($"/apidocs/{containerId}");
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}