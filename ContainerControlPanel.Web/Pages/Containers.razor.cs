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
    private List<ComposeFile> composeFiles { get; set; } = new();
    private List<ImageFile> imageFiles { get; set; } = new();
    private List<Image> images { get; set; } = new();
    private bool loading { get; set; } = false;

    private bool _open;
    private Anchor _anchor;
    private string _width, _height;

    private readonly CancellationTokenSource _cts = new();

    protected override async Task OnInitializedAsync()
    {
        if (NavigationManager.Uri.Split('/').Last() == "")
        {
            NavigationManager.NavigateTo("/containers");
        }

        if (MemoryCache.TryGetValue("lastContainersHref", out string cachedHref))
        {
            NavigationManager.NavigateTo(cachedHref);
        }
        else
        {
            LiveFilter ??= "true";
        }

        await LoadContainers(true);
        await LoadContainersDetails();
        await LoadComposeFiles();
        await LoadImageFiles();
        await LoadImages();

        if (bool.Parse(Configuration["Realtime"]))
        {
            _ = Task.Run(async () =>
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        await LoadContainers(true);
                        await LoadContainersDetails();
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
        }
        else
        {
            if (containers.Count() == 0)
            {
                loading = true;
                this.StateHasChanged();
            }

            var result = await containerAPI.GetContainers(force, liveFilter);
            MemoryCache.Set("containers", result);
            containers = result;          
        }

        loading = false;
        this.StateHasChanged();
    }

    private async Task LoadContainersDetails()
    {
        foreach (var container in containers)
        {
            if (container.Status.Contains("Up") && !MemoryCache.TryGetValue($"containerDetails_{container.ContainerId}", out _))
            {
                ContainerDetails containerDetails = await containerAPI.GetContainerDetails(container.ContainerId);

                MemoryCache.Set($"containerDetails_{container.ContainerId}", containerDetails);
            }
        }
    }

    private async Task LoadComposeFiles()
    {
        composeFiles = await containerAPI.SearchForComposes();
        this.StateHasChanged();
    }

    private async Task LoadImageFiles()
    {
        imageFiles = await containerAPI.SearchForImages();
        this.StateHasChanged();
    }

    private async Task LoadImages()
    {
        images = await containerAPI.GetImages();
        this.StateHasChanged();
    }

    private async Task StopContainer(string containerId, ComposeFile? composeFile)
    {
        var container = containers.Find(x => x.ContainerId == containerId);
        container.Status = "Stopping...";

        try
        {
            if (composeFile != null)
            {
                string context = string.IsNullOrEmpty(Configuration["Context"])
                    ? ""
                    : $" -p {Configuration["Context"]}";
                await containerAPI.ExecuteCommand($"compose -f {composeFile.FilePath}{context} down");
            }            
            else
                await containerAPI.StopContainer(containerId);

            container.Status = "Stopped";
            this.StateHasChanged();

            Snackbar.Clear();
            Snackbar.Configuration.PositionClass = Defaults.Classes.Position.BottomRight;
            Snackbar.Add(Localizer[Locales.Resource.StopContainerSuccess], Severity.Normal);
        }
        catch (Exception ex)
        {
            Snackbar.Clear();
            Snackbar.Configuration.PositionClass = Defaults.Classes.Position.BottomRight;
            Snackbar.Add(Localizer[Locales.Resource.StopContainerError], Severity.Normal);
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

    private async Task<IDialogReference> OpenStartContainerDialogAsync(Container container, ActionType actionType)
    {
        var options = new DialogOptions { CloseOnEscapeKey = true };
        return await DialogService.ShowAsync<StartContainerDialog>(
            "",
            new DialogParameters()
            {
                { "Container", container },
                { 
                  "ComposeFile", 
                  container.Labels.Contains("com.docker.compose.project.config_files=")
                    ? container.Labels.TryGetComposeFile(composeFiles)
                    : container.Names.TryGetComposeFileByServiceName(composeFiles) 
                },
                { "ActionType", actionType }
            },
            options
        );
    }

    private Task OpenComposeFileEditDialogAsync(ComposeFile composeFile)
    {
        var options = new DialogOptions 
        { 
            CloseOnEscapeKey = true,
            FullWidth = true,
            MaxWidth = MaxWidth.Large          
        };

        return DialogService.ShowAsync<ContainerComposeEditDialog>(
            "",
            new DialogParameters()
            {
                { "ComposeFile", composeFile }
            },
            options
        );
    }

    private Task OpenComposesDirectoryDialogAsync()
    {
        var options = new DialogOptions
        {
            CloseOnEscapeKey = true,
            FullWidth = true,
            MaxWidth = MaxWidth.Small
        };

        return DialogService.ShowAsync<DockerComposeDirectoryViewDialog>(
            "",
            new DialogParameters()
            {
                { "ComposeFiles", composeFiles }
            },
            options
        );
    }

    private Task OpenImagesDirectoryDialogAsync()
    {
        var options = new DialogOptions
        {
            CloseOnEscapeKey = true,
            FullWidth = true,
            MaxWidth = MaxWidth.Small
        };

        return DialogService.ShowAsync<DockerImageDirectoryViewDialog>(
            "",
            new DialogParameters()
            {
                { "ImageFiles", imageFiles }
            },
            options
        );
    }

    private Task OpenImagesListDialogAsync()
    {
        var options = new DialogOptions
        {
            CloseOnEscapeKey = true,
            FullWidth = true,
            MaxWidth = MaxWidth.Large
        };

        return DialogService.ShowAsync<DockerImagesListDialog>(
            "",
            new DialogParameters()
            {
                { "Images", images }
            },
            options
        );
    }

    private async Task Refresh()
    {
        await LoadContainers(true);
        await LoadComposeFiles();
        await LoadImageFiles();
        await LoadImages();
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