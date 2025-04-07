using ContainerControlPanel.Domain.Models;
using ContainerControlPanel.Web.Interfaces;
using ContainerControlPanel.Web.Pages;
using ContainerControlPanel.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Plotly.Blazor;
using Plotly.Blazor.LayoutLib;
using Plotly.Blazor.Traces;
using System.Diagnostics;
using System.Threading;

namespace ContainerControlPanel.Web.Components.Metrics;

public partial class Histogram(ITelemetryAPI telemetryAPI) : IDisposable
{
    [Inject]
    WebSocketService WebSocketService { get; set; }

    [Inject]
    IStringLocalizer<Locales.Resource> Localizer { get; set; }

    [Inject]
    IConfiguration Configuration { get; set; }

    [Parameter]
    public Metric Metrics { get; set; }

    private string? CurrentMethod
    {
        get => currentMethod;
        set
        {
            currentMethod = value;
            RefreshHistogram();
        }
    }
    private string? CurrentStatus
    {
        get => currentStatus;
        set
        {
            currentStatus = value;
            RefreshHistogram();
        }
    }

    private PlotlyChart chart;
    private Plotly.Blazor.Config config;
    private Plotly.Blazor.Layout layout;
    private IList<ITrace> data;

    private CancellationTokenSource cancellationToken;

    private event EventHandler<DataJob> DataEvent;

    private string? currentMethod = "all";

    private string? currentStatus = "all";

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            DataEvent += UpdateData;
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    protected override async Task OnInitializedAsync()
    {
        cancellationToken = new CancellationTokenSource();

        config = new Plotly.Blazor.Config
        {
            Responsive = true
        };

        layout = new Plotly.Blazor.Layout
        {
            PaperBgColor = "#2B2B2B",
            PlotBgColor = "#2B2B2B",
            Font = new Font
            {
                Color = "#ffffff",
            },
            Margin = new Margin
            {
                L = 40,
                R = 0,
                B = 50,
                T = 50,
                Pad = 4
            },
            Width = 800,
            Height = 400,
            BarMode = BarModeEnum.Overlay,
            ShowLegend = true,
            XAxis = new List<XAxis>
            {
                new()
                {
                    Type = Plotly.Blazor.LayoutLib.XAxisLib.TypeEnum.Category,
                    GridColor = new object[] { "#ffffff" },
                }
            },
        };

        RefreshHistogram();

        if (bool.Parse(Configuration["Realtime"]))
        {
            WebSocketService.MetricsUpdated += OnMetricsUpdated;
            await WebSocketService.ConnectAsync($"ws://{Configuration["WebAPIHost"]}:5121/ws");
        }

        base.OnInitialized();
    }

    private void RefreshHistogram()
    {
        List<object> explicitBounds = Metrics.Histogram.DataPoints.First().ExplicitBounds.Select(x => (object)x).ToList();
        explicitBounds.Add($"{explicitBounds.Last()}+");
        object[] bucketCounts = new object[Metrics.Histogram.DataPoints.First().BucketCounts.Count()];

        foreach (var item in Metrics.Histogram.DataPoints
            .Where(x => currentMethod == "all" || x.Attributes.Any(x => x.Key == "http.request.method" && x.Value.StringValue == currentMethod))
            .Where(x => currentStatus == "all" || x.Attributes.Any(x => x.Key == "http.response.status_code" && x.Value.IntValue == currentStatus)))
        {
            int[] bucketCount = item.BucketCounts.Select(x => int.Parse(x)).ToArray();

            for (int i = 0; i < bucketCount.Length; i++)
            {
                if (bucketCounts[i] == null)
                {
                    bucketCounts[i] = (int)bucketCount[i];
                }
                else
                {
                    bucketCounts[i] = (int)bucketCounts[i] + (int)bucketCount[i];
                }
            }
        }

        data = new List<ITrace>
        {
            new Bar
            {
                X = explicitBounds.ToList(),
                Y = bucketCounts.ToList(),
                Name = "All",
                Width = 0.70m,
                Marker = new Plotly.Blazor.Traces.BarLib.Marker
                {
                    Color = "#00B577",
                    Opacity = 0.75m
                }
            }
        };

        DataEvent?.Invoke(this, new DataJob(new DataEventArgs(explicitBounds.ToList(), bucketCounts.ToList(), 0), chart, data));
    }

    private async void UpdateData(object sender, DataJob job)
    {
        if ((!job?.DataEventArgs?.X?.Any() ?? true) || (!job?.DataEventArgs?.Y?.Any() ?? true) || job.Chart == null || job.Traces == null || job.DataEventArgs.TraceIndex < 0)
        {
            return;
        }

        var stopwatch = new Stopwatch();
        stopwatch.Restart();

        var count = job.DataEventArgs.Y.Count();

        try
        {
            await InvokeAsync(async () => await job.Chart.ExtendTrace(job.DataEventArgs.X, job.DataEventArgs.Y, job.DataEventArgs.TraceIndex, count, cancellationToken.Token));
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return;
        }

        stopwatch.Stop();
    }

    private void OnMetricsUpdated(ContainerControlPanel.Domain.Models.Metrics metrics)
    {
        if (metrics != null)
        {
            var newMetrics = metrics.ScopeMetrics
                .Find(x => x.Metrics.Find(x => x.Name == Metrics.Name) != null).Metrics
                .Find(x => x.Name == Metrics.Name);

            if (newMetrics != null)
            {
                Metrics = newMetrics;
                RefreshHistogram();
            }
        }
    }

    public void Dispose()
    {
        cancellationToken?.Cancel();
    }
}