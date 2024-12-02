using ContainerControlPanel.Domain.Models;
using Majorsoft.Blazor.Components.Common.JsInterop.Scroll;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Net.Http.Json;

namespace ContainerControlPanel.Web.Pages;

public partial class Logs(HttpClient client)
{
    [Inject]
    IConfiguration Configuration { get; set; }
    [Inject]
    IScrollHandler scrollHandler { get; set; }

    private HttpClient client { get; set; } = client;
    private List<Container> containers { get; set; } = new();
    private string logs = string.Empty;
    private Container _container = null;
    private Container container {
        get => _container;
        set
        {
            _container = value;
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
            firstScroll = false;
            LoadLogs();
        }
    }
    private bool firstScroll { get; set; } = false;

    protected override async Task OnInitializedAsync()
    {
        await LoadContainers(false);

        if (bool.Parse(Configuration["Realtime"]))
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
        containers = await client.GetFromJsonAsync<List<Container>>($"api/getContainersList?force={force}&liveFilter=true");
    }

    private async Task LoadLogs()
    {
        string filterString = string.Empty;
        if (string.IsNullOrWhiteSpace(container?.ContainerId))
        {
            return;
        }

        if (filterDate.HasValue)
        {
            filterString = $"&date={filterDate.Value.ToShortDateString()}";
        }
        else if (!string.IsNullOrWhiteSpace(timestamp))
        {
            filterString = $"&timestamp={timestamp}";
        }

        logs = await client.GetStringAsync($"api/getContainerLogs?containerId={container.ContainerId}{filterString}");
        this.StateHasChanged();

        if (!firstScroll)
        {
            await scrollHandler.ScrollToElementByIdAsync("logs-bottom");
            firstScroll = true;
        }
    }
}