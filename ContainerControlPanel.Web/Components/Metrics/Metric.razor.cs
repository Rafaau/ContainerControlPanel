using ContainerControlPanel.Domain.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Plotly.Blazor.Traces;
using Plotly.Blazor;
using Plotly.Blazor.Traces.ScatterLib;
using Plotly.Blazor.LayoutLib;
using System.Data;
using System.Diagnostics;
using System.Collections.Concurrent;
using ContainerControlPanel.Web.Interfaces;
using ContainerControlPanel.Web.Services;

namespace ContainerControlPanel.Web.Components.Metrics;

public partial class Metric(ITelemetryAPI telemetryAPI) : IDisposable
{
    [Inject]
    WebSocketService WebSocketService { get; set; }

    [Inject]
    IStringLocalizer<Locales.Resource> Localizer { get; set; }

    [Inject]
    IConfiguration Configuration { get; set; }

    [Parameter]
    public ContainerControlPanel.Domain.Models.Metric Metrics { get; set; }

    [Parameter]
    public string ResourceName { get; set; }

    ITelemetryAPI telemetryAPI { get; set; } = telemetryAPI;

    private string? currentDataPoint { get; set; } = "all";

    private List<ContainerControlPanel.Domain.Models.Metric> metrics { get; set; } = new();

    private int timestamp = 5;
    private decimal averageDuration;
    private List<object> minMaxDuration;

    PlotlyChart chart;
    Plotly.Blazor.Config config;
    Plotly.Blazor.Layout layout;
    IList<ITrace> data;
    private CancellationTokenSource cancellationToken;
    private BlockingCollection<DataJob> queue;
    private event EventHandler<DataJob> DataEvent;
    private IndicatorChart averageDurationIndicator;

    private readonly CancellationTokenSource _cts = new();

    protected override async Task OnInitializedAsync()
    {
        cancellationToken = new CancellationTokenSource();
        queue = new BlockingCollection<DataJob>();
        InitializeChart();
        metrics.Add(Metrics);

        if (bool.Parse(Configuration["Realtime"]))
        {
            WebSocketService.MetricsUpdated += OnMetricsUpdated;
            await WebSocketService.ConnectAsync($"ws://{Configuration["WebAPIHost"]}:5121/ws");
        }

        base.OnInitialized();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            DataEvent += UpdateData;
            _ = WriteData();
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    private void InitializeChart()
    {
        config = new Plotly.Blazor.Config
        {
            DisplayModeBar = Plotly.Blazor.ConfigLib.DisplayModeBarEnum.False,
            DisplayLogo = false,
            ShowAxisDragHandles = false
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
            YAxis = new List<YAxis>
            {
                new()
                {
                    AutoRange = Plotly.Blazor.LayoutLib.YAxisLib.AutoRangeEnum.True,
                    Domain = new List<object> { 0, 1, 2, 3 },
                    Range = new List<object> { 0, 0.1, 1, 10 },
                    Type = Plotly.Blazor.LayoutLib.YAxisLib.TypeEnum.Linear,
                    GridColor = new object[] { "#ffffff" },
                }
            },
            ShowLegend = true
        };

        data = new List<ITrace>
        {
            new Scatter
            {
                Name = "Match Attempts",
                Mode = ModeFlag.Lines,

                X = new List<object>{ DateTime.Now.AddMinutes(-timestamp), DateTime.Now },
                Y = new List<object>{2, 2},
                Line = new Line
                {
                    Color = "#00B577",
                    Width = 2
                },
                Fill = FillEnum.ToZeroY,
            }
        };
    }

    private Task WriteData()
    {
        return Task.Run(async () =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var max = new List<object>
                {
                    DateTime.Now.AddMinutes(-timestamp),
                    DateTime.Now.AddMinutes(-timestamp * 0.9),
                    DateTime.Now.AddMinutes(-timestamp * 0.85),
                    DateTime.Now.AddMinutes(-timestamp * 0.8),
                    DateTime.Now.AddMinutes(-timestamp * 0.75),
                    DateTime.Now.AddMinutes(-timestamp * 0.7),
                    DateTime.Now.AddMinutes(-timestamp * 0.65),
                    DateTime.Now.AddMinutes(-timestamp * 0.6),
                    DateTime.Now.AddMinutes(-timestamp * 0.55),
                    DateTime.Now.AddMinutes(-timestamp * 0.5),
                    DateTime.Now.AddMinutes(-timestamp * 0.45),
                    DateTime.Now.AddMinutes(-timestamp * 0.4),
                    DateTime.Now.AddMinutes(-timestamp * 0.35),
                    DateTime.Now.AddMinutes(-timestamp * 0.3),
                    DateTime.Now.AddMinutes(-timestamp * 0.25),
                    DateTime.Now.AddMinutes(-timestamp * 0.2),
                    DateTime.Now.AddMinutes(-timestamp * 0.15),
                    DateTime.Now.AddMinutes(-timestamp * 0.1),
                    DateTime.Now
                };

                int lastValue = 0;

                if (metrics.Last().Histogram != null)
                {
                    lastValue = int.Parse(metrics.Last().Histogram.DataPoints.Last().AsInt);
                }
                else
                {
                    lastValue = int.Parse(metrics.Last().Sum.DataPoints.Last().AsInt);
                }

                var may = new List<object>
                {
                    GetMetricValueByTimestamp(DateTime.Now.AddMinutes(-timestamp), int.Parse(Configuration["TimeOffset"])),
                    GetMetricValueByTimestamp(DateTime.Now.AddMinutes(-timestamp * 0.9), int.Parse(Configuration["TimeOffset"])),
                    GetMetricValueByTimestamp(DateTime.Now.AddMinutes(-timestamp * 0.85), int.Parse(Configuration["TimeOffset"])),
                    GetMetricValueByTimestamp(DateTime.Now.AddMinutes(-timestamp * 0.8), int.Parse(Configuration["TimeOffset"])),
                    GetMetricValueByTimestamp(DateTime.Now.AddMinutes(-timestamp * 0.75), int.Parse(Configuration["TimeOffset"])),
                    GetMetricValueByTimestamp(DateTime.Now.AddMinutes(-timestamp * 0.7), int.Parse(Configuration["TimeOffset"])),
                    GetMetricValueByTimestamp(DateTime.Now.AddMinutes(-timestamp * 0.65), int.Parse(Configuration["TimeOffset"])),
                    GetMetricValueByTimestamp(DateTime.Now.AddMinutes(-timestamp * 0.6), int.Parse(Configuration["TimeOffset"])),
                    GetMetricValueByTimestamp(DateTime.Now.AddMinutes(-timestamp * 0.55), int.Parse(Configuration["TimeOffset"])),
                    GetMetricValueByTimestamp(DateTime.Now.AddMinutes(-timestamp * 0.5), int.Parse(Configuration["TimeOffset"])),
                    GetMetricValueByTimestamp(DateTime.Now.AddMinutes(-timestamp * 0.45), int.Parse(Configuration["TimeOffset"])),
                    GetMetricValueByTimestamp(DateTime.Now.AddMinutes(-timestamp * 0.4), int.Parse(Configuration["TimeOffset"])),
                    GetMetricValueByTimestamp(DateTime.Now.AddMinutes(-timestamp * 0.35), int.Parse(Configuration["TimeOffset"])),
                    GetMetricValueByTimestamp(DateTime.Now.AddMinutes(-timestamp * 0.3), int.Parse(Configuration["TimeOffset"])),
                    GetMetricValueByTimestamp(DateTime.Now.AddMinutes(-timestamp * 0.25), int.Parse(Configuration["TimeOffset"])),
                    GetMetricValueByTimestamp(DateTime.Now.AddMinutes(-timestamp * 0.2), int.Parse(Configuration["TimeOffset"])),
                    GetMetricValueByTimestamp(DateTime.Now.AddMinutes(-timestamp * 0.15), int.Parse(Configuration["TimeOffset"])),
                    GetMetricValueByTimestamp(DateTime.Now.AddMinutes(-timestamp * 0.1), int.Parse(Configuration["TimeOffset"])),
                    lastValue
                };

                DataEvent?.Invoke(this, new DataJob(new DataEventArgs(max, may, 0), chart, data));

                await Task.Delay(TimeSpan.FromSeconds(timestamp != 1 ? 1 : 0.1));
            }
        });
    }

    private int GetMetricValueByTimestamp(DateTime timestamp, int timeOffset)
    {
        bool isHistogram = metrics.Last().Histogram != null;

        if (isHistogram)
        {
            var metric = metrics.LastOrDefault(x => x.Histogram.DataPoints.Any(x => x.Attributes.Any(x => x.Key.Contains("status") && x.Value.StringValue.Contains("success")) 
                        && GetDateTimeFromTimestamp(x.TimeUnixNano, timeOffset) <= timestamp));

            if (metric == null)
            {
                return 0;
            }
            else
            {
                return int.Parse(metric.Histogram.DataPoints
                    .LastOrDefault(x => x.Attributes.Any(x => x.Key.Contains("status") && x.Value.StringValue.Contains("success"))
                            && GetDateTimeFromTimestamp(x.TimeUnixNano, timeOffset) <= timestamp).AsInt);
            }
        }
        else
        {
            var metric = metrics.LastOrDefault(x => x.Sum.DataPoints.Any(x => x.Attributes.Any(x => x.Key.Contains("status") && x.Value.StringValue.Contains("success"))
                        && GetDateTimeFromTimestamp(x.TimeUnixNano, timeOffset) <= timestamp));

            if (metric == null)
            {
                return 0;
            }
            else
            {
                return int.Parse(metric.Sum.DataPoints
                    .LastOrDefault(x => x.Attributes.Any(x => x.Key.Contains("status") && x.Value.StringValue.Contains("success"))
                            && GetDateTimeFromTimestamp(x.TimeUnixNano, timeOffset) <= timestamp).AsInt);
            }
        }
    }

    private DateTime GetDateTimeFromTimestamp(string timestamp, int timeOffset)
    {
        var startTime = decimal.Parse(timestamp);
        long milliseconds = Convert.ToInt64(Math.Round(startTime / 1000000));
        DateTime dateTime = DateTimeOffset.FromUnixTimeMilliseconds(milliseconds).AddHours(timeOffset).DateTime;
        return dateTime;
    }

    private void OnMetricsUpdated(ContainerControlPanel.Domain.Models.Metrics metrics)
    {
        if (metrics != null)
        {
            ContainerControlPanel.Domain.Models.Metric metricToAdd = metrics.ScopeMetrics
                .Find(x => x.Metrics.Any(x => x.Name == this.metrics.First().Name)).Metrics
                .Find(x => x.Name == this.metrics.First().Name);

            if (metricToAdd != null)
            {
                this.metrics.Add(metricToAdd);
            }
            
            this.StateHasChanged();
        }
    }

    private async void UpdateData(object sender, DataJob job)
    {
        if ((!job?.DataEventArgs?.X?.Any() ?? true) || (!job?.DataEventArgs?.Y?.Any() ?? true) || job.Chart == null || job.Traces == null || job.DataEventArgs.TraceIndex < 0)
        {
            return;
        }

        var stopwatch = new Stopwatch();
        stopwatch.Restart();

        var count = job.DataEventArgs.X.Count();

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

    public void Dispose()
    {
        DataEvent -= UpdateData;
        cancellationToken?.Cancel();
    }
}