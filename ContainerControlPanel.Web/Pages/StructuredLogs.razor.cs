using ContainerControlPanel.Domain.Models;
using ContainerControlPanel.Web.Interfaces;
using ContainerControlPanel.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace ContainerControlPanel.Web.Pages;

public partial class StructuredLogs(ITelemetryAPI telemetryAPI)
{
    [Inject]
    IStringLocalizer<Locales.Resource> Localizer { get; set; }

    [Inject]
    WebSocketService WebSocketService { get; set; }

    private ITelemetryAPI telemetryAPI { get; set; } = telemetryAPI;

    private List<LogsRoot> logsRoot { get; set; } = new();

    private string currentResource { get; set; } = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await LoadLogs();
        base.OnInitialized();
    }

    private async Task LoadLogs()
    {
        logsRoot = await telemetryAPI.GetLogs();
        this.StateHasChanged();
    }
}