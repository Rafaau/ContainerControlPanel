using ContainerControlPanel.Domain.Models;
using ContainerControlPanel.Web.Interfaces;
using ContainerControlPanel.Web.Services;
using Microsoft.AspNetCore.Components;
using System.Text;

namespace ContainerControlPanel.Web.Pages;

public partial class Traces(ITelemetryAPI telemetryAPI)
{
    [Inject]
    WebSocketService WebSocketService { get; set; }

    [Inject]
    IServiceProvider ServiceProvider { get; set; }

    [Inject]
    IConfiguration Configuration { get; set; }

    private ITelemetryAPI telemetryAPI { get; set; } = telemetryAPI;

    private List<Root> allTraces { get; set; } = new();

    private string currentResource { get; set; } = "all";

    private bool routesOnly { get; set; } = false;

    protected override async Task OnInitializedAsync()
    {
        allTraces = await telemetryAPI.GetTraces();
        this.StateHasChanged();

        WebSocketService.TracesUpdated += OnTracesUpdated;
        await WebSocketService.ConnectAsync("ws://localhost:5121/ws");
    }

    private void OnTracesUpdated(Root traces)
    {
        if (traces != null)
        {
            allTraces.Add(traces);
            this.StateHasChanged();
        }
    }

    private string GetHexString(string text)
    {
        byte[] byteArray = Encoding.Default.GetBytes(text);
        return BitConverter.ToString(byteArray).Replace("-", "").Substring(0, 6);
    }
}