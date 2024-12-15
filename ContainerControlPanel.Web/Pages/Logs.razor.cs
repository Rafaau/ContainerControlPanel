using ContainerControlPanel.Domain.Models;
using ContainerControlPanel.Web.Interfaces;
using Majorsoft.Blazor.Components.Common.JsInterop.Scroll;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace ContainerControlPanel.Web.Pages;

public partial class Logs(IContainerAPI containerAPI)
{
    [Inject]
    IConfiguration configuration { get; set; }

    [Inject]
    IScrollHandler scrollHandler { get; set; }

    [Inject]
    NavigationManager navManager { get; set; }

    [Inject]
    IStringLocalizer<Locales.Resource> Localizer { get; set; }

    [Parameter]
    public string? ContainerId { get; set; }

    [Parameter]
    public string? Timestamp { get; set; }

    [Parameter]
    public string? FilterDate { get; set; }

    private IContainerAPI containerAPI { get; set; } = containerAPI;
    private List<Container> containers { get; set; } = new();
    private string logs = string.Empty;
    private Container _container = null;
    private Container container {
        get => _container;
        set
        {
            _container = value;         
            navManager.NavigateTo($"{navManager.BaseUri}logs/{value.ContainerId}/{filterDate.Value.ToString("yyyyMMdd")}");
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
    private DateTime? _filterDate = DateTime.Now;
    private DateTime? filterDate
    {
        get => _filterDate;
        set
        {
            _filterDate = value;
            if (ContainerId != null)
                navManager.NavigateTo($"{navManager.BaseUri}logs/{ContainerId}/{value.Value.ToString("yyyyMMdd")}");
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
    private bool enableColors { get; set; } = false;

    protected override async Task OnInitializedAsync()
    {
        await LoadContainers(false);

        if (containers.Count > 0 && ContainerId != null)
        {
            container = containers.Find(c => c.ContainerId == ContainerId);

            if (Timestamp != null)
                timestamp = Timestamp;

            if (FilterDate != null)
                filterDate = DateTime.ParseExact(FilterDate, "yyyyMMdd", null);
        }

        if (bool.Parse(configuration["Realtime"]))
        {
            _ = Task.Run(async () =>
            {
                while (true)
                {                    
                    await LoadLogs();
                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
            });
        }
    }

    private async Task LoadContainers(bool force)
    {
        containers = await containerAPI.GetContainers(force, true);
    }

    private async Task LoadLogs()
    {
        if (string.IsNullOrWhiteSpace(container?.ContainerId))
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

        logs = await containerAPI.GetContainerLogs(container.ContainerId, timestamp, filterDate, timeFrom.ToString(), timeTo.ToString());
        this.StateHasChanged();
     
        if (!firstScroll)
        {
            await scrollHandler.ScrollToElementByIdAsync("logs-bottom");
            firstScroll = true;
        }   
    }
}