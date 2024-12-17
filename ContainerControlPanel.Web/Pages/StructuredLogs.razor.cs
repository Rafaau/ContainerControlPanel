using ContainerControlPanel.Domain.Models;
using ContainerControlPanel.Web.Interfaces;
using ContainerControlPanel.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System.Text;

namespace ContainerControlPanel.Web.Pages;

public partial class StructuredLogs(ITelemetryAPI telemetryAPI)
{
    [Inject]
    IStringLocalizer<Locales.Resource> Localizer { get; set; }

    [Inject]
    WebSocketService WebSocketService { get; set; }

    [Inject]
    IConfiguration Configuration { get; set; }

    private ITelemetryAPI telemetryAPI { get; set; } = telemetryAPI;

    private List<LogsRoot> logsRoot { get; set; } = new();

    private string currentResource { get; set; } = "all";

    private string currentSeverity { get; set; } = "all";

    private DateTime? timestamp { get; set; } = null;

    private string? filterString { get; set; } = null;

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

    private string GetHexString(string text)
    {
        byte[] byteArray = Encoding.Default.GetBytes(text);
        return BitConverter.ToString(byteArray).Replace("-", "").Substring(0, 6);
    }
}