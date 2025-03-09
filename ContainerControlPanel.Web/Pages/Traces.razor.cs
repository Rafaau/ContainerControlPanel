using ContainerControlPanel.Domain.Models;
using ContainerControlPanel.Web.Interfaces;
using ContainerControlPanel.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using MudBlazor;
using System.Text;

namespace ContainerControlPanel.Web.Pages;

public partial class Traces(ITelemetryAPI telemetryAPI) : IAsyncDisposable
{
    [Inject]
    WebSocketService WebSocketService { get; set; }

    [Inject]
    IServiceProvider ServiceProvider { get; set; }

    [Inject]
    IConfiguration Configuration { get; set; }

    [Inject]
    IStringLocalizer<Locales.Resource> Localizer { get; set; }

    [Inject]
    NavigationManager NavigationManager { get; set; } 

    [Inject]
    IMemoryCache MemoryCache { get; set; }

    [Parameter]
    public string? ResourceParameter { get; set; }

    [Parameter]
    public string? TimestampParameter { get; set; }

    [Parameter]
    public string? RoutesOnlyParameter { get; set; }

    private string? currentRoute
    => $"/traces/{currentResource}" +
    $"/{currentTimestamp?.ToString("yyyy-MM-dd") ?? "null"}" +
    $"/{routesOnly}";

    private string currentResource
    {
        get => ResourceParameter ?? "all";
        set
        {
            ResourceParameter = value;
            NavigationManager.NavigateTo(currentRoute);
            MemoryCache.Set("lastTracesHref", currentRoute);
            LoadTraces();
        }
    }

    private bool routesOnly
    {
        get => bool.Parse(RoutesOnlyParameter ?? "false");
        set
        {
            RoutesOnlyParameter = value.ToString();
            NavigationManager.NavigateTo(currentRoute);
            MemoryCache.Set("lastTracesHref", currentRoute);
        }
    }

    private DateTime? currentTimestamp
    {
        get => TimestampParameter != null && TimestampParameter != "null" 
            ? DateTime.Parse(TimestampParameter) 
            : null;
        set
        {
            TimestampParameter = value?.ToString("yyyy-MM-dd");
            NavigationManager.NavigateTo(currentRoute);
            MemoryCache.Set("lastTracesHref", currentRoute);
            LoadTraces();
        }
    }

    private ITelemetryAPI telemetryAPI { get; set; } = telemetryAPI;

    private List<TracesRoot> allTraces { get; set; } = new();

    private readonly CancellationTokenSource _cts = new();

    private int page { get; set; } = 1;

    protected override async Task OnInitializedAsync()
    {
        if (MemoryCache.TryGetValue("lastTracesHref", out string cachedHref))
        {
            NavigationManager.NavigateTo(cachedHref);

            if (cachedHref.Split("/")[3] == "null" && !bool.Parse(Configuration["LazyLoading"]))
                await LoadTraces();
        }
        else
        {
            TimestampParameter ??= DateTime.Now.ToString("yyyy-MM-dd");
            await LoadTraces();
        }

        if (bool.Parse(Configuration["Realtime"]))
        {
            WebSocketService.TracesUpdated += OnTracesUpdated;
            await WebSocketService.ConnectAsync($"ws://{Configuration["WebAPIHost"]}:5121/ws");
        }
    }

    private async Task LoadTraces()
    {
        List<TracesRoot> result = new();
        if (MemoryCache.TryGetValue("traces", out List<TracesRoot> cachedTraces))
        {
            allTraces = cachedTraces;
            this.StateHasChanged();

            if (bool.Parse(Configuration["LazyLoading"]))
            {
                result = await telemetryAPI
                    .GetTraces(int.Parse(
                        Configuration["TimeOffset"]), 
                        currentResource, 
                        currentTimestamp?.ToString(),
                        1,
                        10);
            }
            else
            {
                result = await telemetryAPI
                    .GetTraces(int.Parse(Configuration["TimeOffset"]), currentResource, currentTimestamp?.ToString());
            }
            
            if (result.Count != allTraces.Count)
            {
                MemoryCache.Set("traces", result);
                allTraces = result;
                this.StateHasChanged();
            }
        }
        else
        {
            if (bool.Parse(Configuration["LazyLoading"]))
            {
                result = await telemetryAPI
                    .GetTraces(int.Parse(
                        Configuration["TimeOffset"]),
                        currentResource,
                        currentTimestamp?.ToString(),
                        1,
                        10);
            }
            else
            {
                result = await telemetryAPI
                    .GetTraces(int.Parse(Configuration["TimeOffset"]), currentResource, currentTimestamp?.ToString());
            }

            MemoryCache.Set("traces", result);
            allTraces = result;
            this.StateHasChanged();
        }

        page = 1;
    }

    private void OnTracesUpdated(TracesRoot traces)
    {
        if (traces != null)
        {
            allTraces.Add(traces);
            MemoryCache.Set("traces", allTraces);
            this.StateHasChanged();
        }
    }

    private async Task LoadMoreTraces()
    {
        if (bool.Parse(Configuration["LazyLoading"]))
        {
            page++;
            var result = await telemetryAPI
                    .GetTraces(int.Parse(
                        Configuration["TimeOffset"]),
                        currentResource,
                        currentTimestamp?.ToString(),
                        page,
                        10);
            allTraces.AddRange(result);
            MemoryCache.Set("structuredlogs", allTraces);
            this.StateHasChanged();
        }
    }

    private string GetHexString(string text)
    {
        byte[] byteArray = Encoding.Default.GetBytes(text);
        return BitConverter.ToString(byteArray).Replace("-", "").Substring(0, 6);
    }

    private void LoadTrace(string traceId)
    {
        NavigationManager.NavigateTo($"/trace/{traceId}");
    }

    public ValueTask DisposeAsync()
    {
        WebSocketService.TracesUpdated -= OnTracesUpdated;
        return WebSocketService.DisposeAsync();
    }
}