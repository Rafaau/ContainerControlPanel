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

    private List<Log> logs { get; set; } = new();

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
            LoadLogs();
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
            if (bool.Parse(Configuration["LazyLoading"]))
            {
                LoadLogs();
            }
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
            if (bool.Parse(Configuration["LazyLoading"]))
            {
                LoadLogs();
            }
        }
    }

    private DateTime? currentTimestamp
    {
        get => TimestampParameter != null && TimestampParameter != "null" 
            ? DateTime.Parse(TimestampParameter) 
            : null;
        set
        {
            TimestampParameter = value?.ToString("yyyy-MM-dd") ?? "null";
            NavigationManager.NavigateTo(currentRoute);
            MemoryCache.Set("lastStructuredLogsHref", currentRoute);
            LoadLogs();
        }
    }

    private bool detailsDrawer { get; set; } = false;

    private Log currentLog { get; set; } = null;

    private readonly CancellationTokenSource _cts = new();

    private int page { get; set; } = 1;

    protected override async Task OnInitializedAsync()
    {
        if (MemoryCache.TryGetValue("lastStructuredLogsHref", out string cachedHref))
        {
            NavigationManager.NavigateTo(cachedHref);

            if (cachedHref.Split("/")[3] == "null" && !bool.Parse(Configuration["LazyLoading"]))
                await LoadLogs();
        }
        else
        {
            TimestampParameter ??= DateTime.Now.ToString("yyyy-MM-dd");
            await LoadLogs();
        }
      
        if (bool.Parse(Configuration["Realtime"]))
        {
            WebSocketService.LogsUpdated += OnLogsUpdated;
            await WebSocketService.ConnectAsync($"ws://{Configuration["WebAPIHost"]}:5121/ws");
        }      
    }

    private async Task LoadLogs()
    {
        List<Log> result;

        if (MemoryCache.TryGetValue("structuredlogs", out List<Log> cachedLogs))
        {
            logs = cachedLogs;
            this.StateHasChanged();

            if (bool.Parse(Configuration["LazyLoading"]))
            {
                result = await telemetryAPI.GetLogs(
                    int.Parse(Configuration["TimeOffset"]),
                    currentTimestamp?.ToString() ?? "null",
                    currentResource,
                    currentSeverity,
                    currentFilter,
                    1,
                    10);
            }
            else
            {
                result = await telemetryAPI.GetLogs(
                    int.Parse(Configuration["TimeOffset"]),
                    currentTimestamp?.ToString() ?? "null",
                    currentResource);
            }

            if (result.Count != logs.Count)
            {
                MemoryCache.Set("structuredlogs", result);
                logs = result;
                this.StateHasChanged();
            }
        }
        else
        {
            if (bool.Parse(Configuration["LazyLoading"]))
            {
                result = await telemetryAPI.GetLogs(
                    int.Parse(Configuration["TimeOffset"]),
                    currentTimestamp?.ToString() ?? "null",
                    currentResource,
                    currentSeverity,
                    currentFilter,
                    1,
                    10);
            }
            else
            {
                result = await telemetryAPI.GetLogs(
                    int.Parse(Configuration["TimeOffset"]), 
                    currentTimestamp?.ToString() ?? "null", 
                    currentResource);
            }
            
            MemoryCache.Set("structuredlogs", result);
            logs = result;
            this.StateHasChanged();
        }

        page = 1;
    }

    private void OnLogsUpdated(Log log)
    {
        if (log != null && !log.ContainsReqRes())
        {
            logs.Add(log);
            MemoryCache.Set("structuredlogs", logs);
            this.StateHasChanged();
        }
    }

    private async Task LoadMoreLogs()
    {
        if (bool.Parse(Configuration["LazyLoading"]))
        {
            page++;
            var result = await telemetryAPI.GetLogs(
                int.Parse(Configuration["TimeOffset"]),
                currentTimestamp?.ToString() ?? "null",
                currentResource,
                currentSeverity,
                currentFilter,
                page,
                10);
            logs.AddRange(result);
            MemoryCache.Set("structuredlogs", logs);
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