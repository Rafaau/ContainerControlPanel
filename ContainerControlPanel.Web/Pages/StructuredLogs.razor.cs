using ContainerControlPanel.Domain.Models;
using ContainerControlPanel.Web.Interfaces;
using ContainerControlPanel.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using System.Text;

namespace ContainerControlPanel.Web.Pages;

public partial class StructuredLogs(ITelemetryAPI telemetryAPI) : IAsyncDisposable
{
    [Inject]
    IStringLocalizer<Locales.Resource> Localizer { get; set; }

    [Inject]
    WebSocketService WebSocketService { get; set; }

    [Inject]
    IConfiguration Configuration { get; set; }

    [Inject]
    IMemoryCache MemoryCache { get; set; }

    [Inject]
    NavigationManager NavigationManager { get; set; }

    [Parameter]
    public string? ResourceParameter { get; set; }

    [Parameter]
    public string? SeverityParameter { get; set; }

    [Parameter]
    public string? FilterParameter { get; set; }

    [Parameter]
    public string? TimestampParameter { get; set; }

    private ITelemetryAPI telemetryAPI { get; set; } = telemetryAPI;

    private List<LogsRoot> logsRoot { get; set; } = new();

    private string? currentRoute =>
        $"/structuredlogs/{currentResource}" +
        $"/{currentTimestamp?.ToString("yyyy-MM-dd") ?? "null"}" +
        $"/{currentSeverity}" +
        $"/{currentFilter}";

    private string? currentResource
    {
        get => ResourceParameter ?? "all";
        set
        {
            ResourceParameter = value;
            NavigationManager.NavigateTo(currentRoute);
            MemoryCache.Set("lastStructuredLogsHref", currentRoute);
        }
    }

    private string? currentSeverity
    {
        get => SeverityParameter ?? "all";
        set
        {
            SeverityParameter = value;
            NavigationManager.NavigateTo(currentRoute);
            MemoryCache.Set("lastStructuredLogsHref", currentRoute);
        }
    }

    private string? currentFilter
    {
        get => FilterParameter ?? string.Empty;
        set
        {
            FilterParameter = value;
            NavigationManager.NavigateTo(currentRoute);
            MemoryCache.Set("lastStructuredLogsHref", currentRoute);
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
            MemoryCache.Set("lastStructuredLogsHref", currentRoute);
        }
    }

    private bool detailsDrawer { get; set; } = false;

    private LogDetailsView currentLog { get; set; } = null;

    private readonly CancellationTokenSource _cts = new();

    protected override async Task OnInitializedAsync()
    {
        if (MemoryCache.TryGetValue("lastStructuredLogsHref", out string cachedHref))
        {
            NavigationManager.NavigateTo(cachedHref);
        }
        else
        {
            TimestampParameter ??= DateTime.Now.ToString("yyyy-MM-dd");
        }
        
        await LoadLogs();

        if (bool.Parse(Configuration["Realtime"]))
        {
            WebSocketService.LogsUpdated += OnLogsUpdated;
            await WebSocketService.ConnectAsync("ws://localhost:5121/ws");
        }      
    }

    private async Task LoadLogs()
    {
        if (MemoryCache.TryGetValue("structuredlogs", out List<LogsRoot> cachedLogs))
        {
            logsRoot = cachedLogs;
            this.StateHasChanged();

            var result = await telemetryAPI.GetLogs();

            if (result.Count != logsRoot.Count)
            {
                MemoryCache.Set("structuredlogs", result);
                logsRoot = result;
                this.StateHasChanged();
            }
        }
        else
        {
            var result = await telemetryAPI.GetLogs();
            MemoryCache.Set("structuredlogs", result);
            logsRoot = result;
            this.StateHasChanged();
        }
    }

    private void OnLogsUpdated(LogsRoot log)
    {
        if (log != null)
        {
            logsRoot.Add(log);
            MemoryCache.Set("structuredlogs", logsRoot);
            this.StateHasChanged();
        }
    }

    private string GetHexString(string text)
    {
        byte[] byteArray = Encoding.Default.GetBytes(text);
        return BitConverter.ToString(byteArray).Replace("-", "").Substring(0, 6);
    }

    public ValueTask DisposeAsync()
    {
        WebSocketService.LogsUpdated -= OnLogsUpdated;
        return WebSocketService.DisposeAsync();
    }
}