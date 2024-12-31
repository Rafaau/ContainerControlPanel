using ContainerControlPanel.Domain.Models;
using ContainerControlPanel.Web.Interfaces;
using Majorsoft.Blazor.Components.Common.JsInterop.Scroll;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;

namespace ContainerControlPanel.Web.Pages;

public partial class Logs(IContainerAPI containerAPI) : IDisposable
{
    [Inject]
    IConfiguration configuration { get; set; }

    [Inject]
    IScrollHandler scrollHandler { get; set; }

    [Inject]
    NavigationManager NavigationManager { get; set; }

    [Inject]
    IStringLocalizer<Locales.Resource> Localizer { get; set; }

    [Inject]
    IMemoryCache MemoryCache { get; set; }

    [Parameter]
    public string? ContainerId { get; set; }

    [Parameter]
    public string? Timestamp { get; set; }

    [Parameter]
    public string? FilterDate { get; set; }

    private string currentRoute
        => $"/logs/{containerId ?? "null"}/{filterDate?.ToString("yyyy-MM-dd") ?? "null"}";

    private IContainerAPI containerAPI { get; set; } = containerAPI;
    private List<Container> containers { get; set; } = new();
    private string logs = string.Empty;

    private string? containerId
    {
        get => ContainerId != null && ContainerId != "null"
            ? ContainerId
            : null;
        set
        {
            ContainerId = value;
            NavigationManager.NavigateTo(currentRoute);
            MemoryCache.Set("lastLogsHref", currentRoute);
            firstScroll = false;
            LoadLogs();
        }
    }
    private string _timestamp = string.Empty;
    private string timestamp 
    { 
        get => _timestamp;
        set
        {
            _timestamp = value;
            filterDate = DateTime.Now;
            firstScroll = false;
            LoadLogs();
        }
    }
    private DateTime? filterDate
    {
        get => FilterDate != null && FilterDate != "null" 
            ? DateTime.ParseExact(FilterDate, "yyyy-MM-dd", null) 
            : null;
        set
        {
            FilterDate = value?.ToString("yyyy-MM-dd");
            NavigationManager.NavigateTo(currentRoute);
            MemoryCache.Set("lastLogsHref", currentRoute);
            firstScroll = false;
            LoadLogs();
        }
    }
    private TimeSpan? _timeFrom = new TimeSpan(0, 0, 0);
    private TimeSpan? _timeTo = new TimeSpan(23, 59, 59);
    private TimeSpan? timeFrom
    {
        get => _timeFrom;
        set
        {
            _timeFrom = value;
            firstScroll = false;
            LoadLogs();
        }
    }
    private TimeSpan? timeTo
    {
        get => _timeTo;
        set
        {
            _timeTo = value;
            firstScroll = false;
            LoadLogs();
        }
    }
    private bool firstScroll { get; set; } = false;

    private readonly CancellationTokenSource _cts = new();

    protected override async Task OnInitializedAsync()
    {
        await LoadContainers(false);

        if (MemoryCache.TryGetValue("lastLogsHref", out string cachedHref))
        {
            NavigationManager.NavigateTo(cachedHref);
        }
        else
        {
            FilterDate ??= DateTime.Now.ToString("yyyy-MM-dd");
        }

        if (bool.Parse(configuration["Realtime"]))
        {
            _ = Task.Run(async () =>
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        await LoadLogs();
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
        if (MemoryCache.TryGetValue("containers", out List<Container> cachedContainers) && !force)
        {
            containers = cachedContainers;
            this.StateHasChanged();

            var result = await containerAPI.GetContainers(force, true);

            if (result.Count != containers.Count)
            {
                MemoryCache.Set("containers", result);
                containers = result;
                this.StateHasChanged();
            }
        }
        else
        {
            var result = await containerAPI.GetContainers(force, true);
            MemoryCache.Set("containers", result);
            containers = result;
            this.StateHasChanged();
        }
    }

    private async Task LoadLogs()
    {
        if (logs.Count() == 0
            && MemoryCache.TryGetValue("logs", out string cachedLogs))
        {
            logs = cachedLogs;
            this.StateHasChanged();
        }

        if (string.IsNullOrWhiteSpace(containerId))
        {
            return;
        }

        string filterString = string.Empty;

        if (!string.IsNullOrWhiteSpace(timestamp))
        {
            filterString += $"&timestamp={timestamp}";
        }
        else if (filterDate.HasValue)
        {
            filterString += $"&date={filterDate.Value.ToShortDateString()}&timeFrom={timeFrom.ToString()}&timeTo={timeTo.ToString()}";
        }

        logs = await containerAPI.GetContainerLogs(containerId, timestamp, filterDate, timeFrom.ToString(), timeTo.ToString());
        this.StateHasChanged();

        MemoryCache.Set("logs", logs);

        if (!firstScroll)
        {
            await scrollHandler.ScrollToElementByIdAsync("logs-bottom");
            firstScroll = true;
        }   
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}