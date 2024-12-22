using ContainerControlPanel.Domain.Models;
using ContainerControlPanel.Web.Interfaces;
using ContainerControlPanel.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
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
        }
    }

    private bool routesOnly
    {
        get => bool.Parse(RoutesOnlyParameter ?? "false");
        set
        {
            RoutesOnlyParameter = value.ToString();
            NavigationManager.NavigateTo(currentRoute);
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
        }
    }

    private ITelemetryAPI telemetryAPI { get; set; } = telemetryAPI;

    private List<TracesRoot> allTraces { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        TimestampParameter ??= DateTime.Now.ToString("yyyy-MM-dd");
        await LoadTraces();

        WebSocketService.TracesUpdated += OnTracesUpdated;
        await WebSocketService.ConnectAsync("ws://localhost:5121/ws");
    }

    private async Task LoadTraces()
    {
        if (MemoryCache.TryGetValue("traces", out List<TracesRoot> cachedTraces))
        {
            allTraces = cachedTraces;
            this.StateHasChanged();

            var result = await telemetryAPI.GetTraces();

            if (result.Count != allTraces.Count)
            {
                MemoryCache.Set("traces", result);
                allTraces = result;
                this.StateHasChanged();
            }
        }
        else
        {
            var result = await telemetryAPI.GetTraces();
            MemoryCache.Set("traces", result);
            allTraces = result;
            this.StateHasChanged();
        }
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