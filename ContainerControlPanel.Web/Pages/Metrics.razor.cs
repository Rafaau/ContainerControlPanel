using ContainerControlPanel.Domain.Models;
using ContainerControlPanel.Web.Components.Metrics;
using ContainerControlPanel.Web.Interfaces;
using ContainerControlPanel.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace ContainerControlPanel.Web.Pages;

public partial class Metrics(ITelemetryAPI telemetryAPI) : IAsyncDisposable
{
    [Inject]
    IStringLocalizer<Locales.Resource> Localizer { get; set; }

    [Inject]
    WebSocketService WebSocketService { get; set; }

    private List<MetricsRoot> allMetrics { get; set; } = new();

    private string? currentResource { get; set; } = null;

    private Metric? currentMetric { get; set; } = null;

    private ITelemetryAPI telemetryAPI { get; set; } = telemetryAPI;

    protected override async Task OnInitializedAsync()
    {
        allMetrics = await telemetryAPI.GetMetrics();
        this.StateHasChanged();

        WebSocketService.MetricsUpdated += OnMetricsUpdated;
        await WebSocketService.ConnectAsync("ws://localhost:5121/ws");
    }

    private void OnMetricsUpdated(MetricsRoot metricsRoot)
    {
        if (metricsRoot != null)
        {
            if (allMetrics.GetResources().GetResourceNames().Contains(metricsRoot.GetResource().GetResourceName()))
            {
                var current = allMetrics.FirstOrDefault(x => x.GetResource().GetResourceName() == metricsRoot.GetResource().GetResourceName());

                if (current != null)
                {
                    allMetrics.Remove(current);
                    allMetrics.Add(metricsRoot);

                    if (currentResource == metricsRoot.GetResource().GetResourceName())
                    {
                        currentMetric = metricsRoot.GetMetrics("http.server.request.duration")[0];
                    }
                }
            }
            else
            {
                allMetrics.Add(metricsRoot);
            }

            this.StateHasChanged();
        }
    }

    public ValueTask DisposeAsync()
    {
        WebSocketService.MetricsUpdated -= OnMetricsUpdated;
        return ValueTask.CompletedTask;
    }
}