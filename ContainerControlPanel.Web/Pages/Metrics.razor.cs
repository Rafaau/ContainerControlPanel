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

    [Inject]
    IConfiguration Configuration { get; set; }

    private List<ContainerControlPanel.Domain.Models.Metrics> allMetrics { get; set; } = new();

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

            return allMetrics
                .FirstOrDefault(x => x.ResourceName == currentResource)?.ScopeMetrics
                .FirstOrDefault().Metrics
                .FirstOrDefault(x => x.Name == MetricParameter);
        }
        set
        {
            MetricParameter = value?.Name;
            NavigationManager.NavigateTo(currentRoute);
            MemoryCache.Set("lastMetricsHref", currentRoute);
        }
    }

    private readonly List<string> compatibleMetrics = new()
    {
        "http.server.request.duration",
        "aspnetcore.routing.match_attempts",
    };

    private readonly CancellationTokenSource _cts = new();

    protected override async Task OnInitializedAsync()
    {
        if (MemoryCache.TryGetValue("lastMetricsHref", out string cachedHref))
        {
            NavigationManager.NavigateTo(cachedHref);
        }

        allMetrics = await telemetryAPI.GetMetrics();
        this.StateHasChanged();

        if (bool.Parse(Configuration["Realtime"]))
        {
            WebSocketService.MetricsUpdated += OnMetricsUpdated;
            await WebSocketService.ConnectAsync($"ws://{Configuration["WebAPIHost"]}:5121/ws");
        }
    }

    private void OnMetricsUpdated(ContainerControlPanel.Domain.Models.Metrics metrics)
    {
        if (metrics != null)
        {
            if (allMetrics.Any(x => x.ResourceName == metrics.ResourceName))
            {
                var current = allMetrics.FirstOrDefault(x => x.ResourceName == metrics.ResourceName);

                if (current != null)
                {
                    allMetrics.Remove(current);
                    allMetrics.Add(metrics);
                }
            }
            else
            {
                allMetrics.Add(metrics);
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