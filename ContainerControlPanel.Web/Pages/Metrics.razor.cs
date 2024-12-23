using ContainerControlPanel.Domain.Models;
using ContainerControlPanel.Web.Interfaces;
using ContainerControlPanel.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;

namespace ContainerControlPanel.Web.Pages;

public partial class Metrics(ITelemetryAPI telemetryAPI) : IAsyncDisposable
{
    [Inject]
    IStringLocalizer<Locales.Resource> Localizer { get; set; }

    [Inject]
    WebSocketService WebSocketService { get; set; }

    [Inject]
    NavigationManager NavigationManager { get; set; }

    [Inject]
    IMemoryCache MemoryCache { get; set; }

    private List<MetricsRoot> allMetrics { get; set; } = new();

    private ITelemetryAPI telemetryAPI { get; set; } = telemetryAPI;

    [Parameter]
    public string? ResourceParameter { get; set; }

    [Parameter]
    public string? MetricParameter { get; set; }

    private string currentRoute
        => $"/metrics/{currentResource}/{currentMetric?.Name.Replace(".", "-") ?? "null"}";

    private string? currentResource
    {
        get => ResourceParameter ?? null;
        set
        {
            ResourceParameter = value;
            NavigationManager.NavigateTo(currentRoute);
            MemoryCache.Set("lastMetricsHref", currentRoute);
        }
    }

    private Metric? currentMetric
    {
        get
        {
            if (MetricParameter == null || MetricParameter == "null")
            {
                return null;
            }

            return allMetrics.FirstOrDefault(x => x.GetResource().GetResourceName() == currentResource)?.GetMetrics(MetricParameter.Replace("-", "."))[0];
        }
        set
        {
            MetricParameter = value?.Name;
            NavigationManager.NavigateTo(currentRoute);
            MemoryCache.Set("lastMetricsHref", currentRoute);
        }
    }

    protected override async Task OnInitializedAsync()
    {
        if (MemoryCache.TryGetValue("lastMetricsHref", out string cachedHref))
        {
            NavigationManager.NavigateTo(cachedHref);
        }

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