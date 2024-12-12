using ContainerControlPanel.Domain.Models;
using ContainerControlPanel.Web.Interfaces;
using ContainerControlPanel.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace ContainerControlPanel.Web.Pages;

public partial class Metrics(ITelemetryAPI telemetryAPI)
{
    [Inject]
    IStringLocalizer<Locales.Resource> Localizer { get; set; }

    [Inject]
    WebSocketService WebSocketService { get; set; }

    private List<MetricsRoot> allMetrics { get; set; } = new();

    private Resource? currentResource { get; set; } = null;

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
            if (allMetrics.GetResources().Contains(metricsRoot.GetResource()))
            {
                var current = allMetrics.FirstOrDefault(x => x.GetResource() == metricsRoot.GetResource());

                if (current != null)
                {
                    allMetrics.Remove(current);
                    allMetrics.Add(metricsRoot);
                }
            }
            else
            {
                allMetrics.Add(metricsRoot);
            }

            this.StateHasChanged();
        }
    }
}