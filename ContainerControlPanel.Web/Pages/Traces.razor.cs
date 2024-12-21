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

    private ITelemetryAPI telemetryAPI { get; set; } = telemetryAPI;

    private List<TracesRoot> allTraces { get; set; } = new();

    private string currentResource { get; set; } = "all";

    private bool routesOnly { get; set; } = false;

    private DateTime? timestamp { get; set; } = DateTime.Now;

    protected override async Task OnInitializedAsync()
    {
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