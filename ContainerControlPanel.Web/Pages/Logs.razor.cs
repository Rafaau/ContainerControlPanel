using ContainerControlPanel.Domain.Models;
using Majorsoft.Blazor.Components.Common.JsInterop.Scroll;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Net.Http.Json;

namespace ContainerControlPanel.Web.Pages;

public partial class Logs
{
    [Inject]
    private IJSRuntime jsRuntime { get; set; }
    [Inject]
    private IScrollHandler scrollHandler { get; set; }

    private HttpClient client { get; set; }
    private List<Container> containers { get; set; } = new();
    private string logs = string.Empty;

    private string containerId { get; set; } = string.Empty;
    private string timestamp { get; set; } = string.Empty;
    private DateTime? filterDate { get; set; } = null;

    public Logs(HttpClient client)
    {
        this.client = client;
    }


    protected override async Task OnInitializedAsync()
    {
        await LoadContainers(false);
    }

    private async Task LoadContainers(bool force)
    {
        containers = await client.GetFromJsonAsync<List<Container>>($"api/getContainersList?force={force}&liveFilter=true");
    }

    private async Task LoadLogs()
    {
        string filterString = string.Empty;
        if (string.IsNullOrWhiteSpace(containerId))
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

        logs = await client.GetStringAsync($"api/getContainerLogs?containerId={containerId}{filterString}");
        this.StateHasChanged();

        await scrollHandler.ScrollToElementByIdAsync("logs-bottom");
    }
}