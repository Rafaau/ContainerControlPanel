using ContainerControlPanel.Domain.Models;
using ContainerControlPanel.Web.Interfaces;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using System.Text.Json;
using Microsoft.JSInterop;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Caching.Memory;

namespace ContainerControlPanel.Web.Pages;

public partial class ApiDocs(IContainerAPI containerAPI)
{
    [Inject]
    IStringLocalizer<Locales.Resource> Localizer { get; set; }

    [Inject]
    IJSRuntime JS { get; set; }

    [Inject]
    IMemoryCache MemoryCache { get; set; }

    [Inject]
    NavigationManager NavigationManager { get; set; }

    [Parameter]
    public string ContainerId { get; set; }

    private List<Container> containers { get; set; } = new();

    private ContainerDetails containerDetails { get; set; }

    private List<ControllerView> controllers { get; set; } = new();

    private IContainerAPI containerAPI { get; set; } = containerAPI;

    private HttpClient httpClient { get; set; }

    private string? containerId
    {
        get => ContainerId ?? null;
        set
        {
            ContainerId = value;
            MemoryCache.Set("lastApiDocsHref", $"/apidocs/{value}");
            NavigationManager.NavigateTo($"/apidocs/{value}");
            LoadData();
        }
    }

    private string formattedRequestJson { get; set; }

    private string formattedResponseJson { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await LoadContainers();

        if (MemoryCache.TryGetValue("lastApiDocsHref", out string lastApiDocsHref))
        {
            NavigationManager.NavigateTo(lastApiDocsHref);
        }      

        if (ContainerId != null)
        {
            MemoryCache.Set("lastApiDocsHref", $"/apidocs/{ContainerId}");
            await LoadData();
        }       
    }

    private async Task LoadData()
    {
        if (MemoryCache.TryGetValue($"endpoints_{ContainerId}", out List<ControllerView> cachedControllers))
        {
            controllers = cachedControllers;
            this.StateHasChanged();
        }

        containerDetails = await containerAPI.GetContainerDetails(ContainerId);
        var port = containerDetails.NetworkSettings.Ports.FirstOrDefault().Value.FirstOrDefault().HostPort;

        if (port != null)
        {
            httpClient = new HttpClient
            {
                BaseAddress = new Uri($"http://localhost:{port}")
            };

            await LoadEndpoints();
        }
    }

    private async Task LoadContainers()
    {
        var result = await containerAPI.GetContainers(true, true);
        containers = result.Where(c => IsASPNETCoreContainer(c.ContainerId)).ToList();
        MemoryCache.Set("apiDocsContainers", containers);

        this.StateHasChanged();
    }

    private async Task LoadEndpoints()
    {
        var docs = await httpClient.GetFromJsonAsync<List<EndpointInfo>>("docs");

        if (docs != null)
        {
            controllers = docs.GetControllers();

            foreach (var action in controllers.SelectMany(c => c.Actions))
            {
                action.RequestBodyFormatted = await FormatJson(action.Parameters.Find(p => p.Source == "Body")?.Type?.GetRequestBodyJson() ?? "");
                action.ResponseBodyFormatted = await FormatJson(action.ReturnType.GetResponseBodyJson());
            }

            MemoryCache.Set($"endpoints_{ContainerId}", controllers);
        }

        this.StateHasChanged();
    }

    private bool IsASPNETCoreContainer(string containerId)
    {
        if (MemoryCache.TryGetValue($"containerDetails_{containerId}", out ContainerDetails _containerDetails))
        {
            return _containerDetails.Config.EnvironmentVariables.Any(x => x.Name.Contains("ASPNETCORE"));
        }
        else
        {
            return false;
        }
    }

    private async Task<string> FormatJson(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return string.Empty;
        }

        var preJson = JsonObject.Parse(body);
        var json = JsonSerializer.Serialize(preJson, new JsonSerializerOptions { WriteIndented = true });
        return await JS.InvokeAsync<string>("colorizeJson", json);
    }
}