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
using MudBlazor.Extensions;
using ContainerControlPanel.Web.Services;

namespace ContainerControlPanel.Web.Components.Metrics;

public partial class RequestDuration(ITelemetryAPI telemetryAPI) : IDisposable
{
    [Inject]
    WebSocketService WebSocketService { get; set; }

    [Inject]
    IStringLocalizer<Locales.Resource> Localizer { get; set; }

    [Inject]
    IConfiguration Configuration { get; set; }

    [Parameter]
    public Metric Metric { get; set; }

    [Parameter]
    public string ResourceName { get; set; }

    ITelemetryAPI telemetryAPI { get; set; } = telemetryAPI;

    private DataPoint? currentDataPoint { get; set; } = null;

    PlotlyChart chart;
    Plotly.Blazor.Config config;
    Plotly.Blazor.Layout layout;
    IList<ITrace> data;
    private CancellationTokenSource cancellationToken;
    private BlockingCollection<DataJob> queue;
    private event EventHandler<DataJob> DataEvent;
    private int timestamp = 5;
    private List<ResourceSpan> traces;
    private decimal averageDuration;
    private List<object> minMaxDuration;
    private IndicatorChart averageDurationIndicator;

    private readonly CancellationTokenSource _cts = new();

    protected override async Task OnInitializedAsync()
    {
        currentDataPoint = Metric.Histogram.DataPoints.Find(dp => dp.Attributes.Any(a => a.Key == "http.route"));

        cancellationToken = new CancellationTokenSource();
        queue = new BlockingCollection<DataJob>();
        InitializeChart();
        await GetTraces();
        CalculateAverageDuration();

        if (bool.Parse(Configuration["Realtime"]))
        {
            WebSocketService.TracesUpdated += OnTracesUpdated;
            await WebSocketService.ConnectAsync("ws://localhost:5121/ws");
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
            Width = 700,
            Height = 400,
            YAxis = new List<YAxis>
            {
                new()
                {
                    AutoRange = Plotly.Blazor.LayoutLib.YAxisLib.AutoRangeEnum.True,
                    Domain = new List<object> { 0, 1, 2, 3 },
                    Range = new List<object> { 0, 0.1, 1, 10 },
                    Type = Plotly.Blazor.LayoutLib.YAxisLib.TypeEnum.Linear
                }        
            }
        };

        data = new List<ITrace>
        {
            new Scatter
            {
                Name = "P50",
                Mode = ModeFlag.Lines,

                X = new List<object>{ DateTime.Now.AddMinutes(-timestamp), DateTime.Now },
                Y = new List<object>{2, 2},
                Line = new Line
                {
                    Color = "#009c1e",
                    Width = 2
                },
                FillColor = "#009c1e"
            },
            new Scatter
            {
                Name = "P90",
                Mode = ModeFlag.Lines,

                X = new List<object>{ DateTime.Now.AddMinutes(-timestamp), DateTime.Now },
                Y = new List<object>{2, 2},
                Line = new Line
                {
                    Color = "#dd6800",
                    Width = 2
                },
                FillColor = "#dd6800"
            },
            new Scatter
            {
                Name = "P99",
                Mode = ModeFlag.Lines,

                X = new List<object>{ DateTime.Now.AddMinutes(-timestamp), DateTime.Now },
                Y = new List<object>{2, 2},
                Line = new Line
                {
                    Color = "#b00000",
                    Width = 2
                }
            }
        };
    }

    private Task WriteData()
    {
        return Task.Run(async () =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var px50 = new List<object>
                {
                    DateTime.Now.AddMinutes(-timestamp),
                    DateTime.Now
                };

                var py50 = new List<object>
                {
                    Double.Parse(currentDataPoint.CalculateP50Seconds()),
                    Double.Parse(currentDataPoint.CalculateP50Seconds())
                };

                var px90 = new List<object>
                {
                    DateTime.Now.AddMinutes(-timestamp),
                    DateTime.Now
                };

                var py90 = new List<object>
                {
                    Double.Parse(currentDataPoint.CalculateP90Seconds()),
                    Double.Parse(currentDataPoint.CalculateP90Seconds())
                };

                var px99 = new List<object>
                {
                    DateTime.Now.AddMinutes(-timestamp),
                    DateTime.Now
                };

                var py99 = new List<object>
                {
                    Double.Parse(currentDataPoint.CalculateP99Seconds()),
                    Double.Parse(currentDataPoint.CalculateP99Seconds())
                };

                DataEvent?.Invoke(this, new DataJob(new DataEventArgs(px50, py50, 0), chart, data));
                DataEvent?.Invoke(this, new DataJob(new DataEventArgs(px90, py90, 1), chart, data));
                DataEvent?.Invoke(this, new DataJob(new DataEventArgs(px99, py99, 2), chart, data));

                List<Scatter> requestMarkers = await AssignDataJobs();
                List<int> tracesToRemove = new();

                try
                {
                    if (data.Count > 3)
                    {
                        if (requestMarkers.Count == 0)
                        {
                            for (int i = 3; i <= data.Count; i++)
                            {
                                await InvokeAsync(async () => await chart.DeleteTrace(3));
                            }
                        }
                        else
                        {
                            foreach (Scatter removedMarker in data.Where(d => d.As<Scatter>().Meta != null))
                            {
                                if (requestMarkers.Any(r => r.Meta == removedMarker.Meta)) continue;

                                int traceIndex = data.IndexOf(removedMarker);
                                tracesToRemove.Add(traceIndex);
                            }
                        }
                    }

                    if (tracesToRemove.Count > 0)
                    {
                        tracesToRemove = tracesToRemove.OrderByDescending(index => index).ToList();

                        foreach (int traceIndex in tracesToRemove)
                        {
                            await InvokeAsync(async () => await chart.DeleteTrace(traceIndex));
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

                await Task.Delay(TimeSpan.FromSeconds(timestamp != 1 ? 1 : 0.1));
            }
        });
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
     
    private async Task GetTraces()
    {
        var result = await telemetryAPI
            .GetTraces(int.Parse(Configuration["TimeOffset"]), ResourceName);
        traces = result.GetTracesList(routesOnly: true);
    }

    private async Task<List<Scatter>> AssignDataJobs()
    {
        var result = new List<Scatter>();

        if (traces == null || traces.Count == 0) return result;

        await Task.Run(() =>
        {
            foreach (var (trace, index) in traces.Select((trace, index) => (trace, index))
                .Where(x => x.trace.GetTraceRoute().Contains(currentDataPoint.GetRouteName()) 
                    && x.trace.GetTimestamp(int.Parse(Configuration["TimeOffset"])) >= DateTime.Now.AddMinutes(-timestamp)))
            {
                var x = new List<object>
                {
                    trace.GetTimestamp(int.Parse(Configuration["TimeOffset"])),
                };

                double durationInMs = trace.GetDuration().Milliseconds;
                double durationInSeconds = durationInMs / 1000;

                var y = new List<object>
                {
                    durationInSeconds
                };

                var newScatter = new Scatter
                {
                    Name = trace.GetTraceName(),
                    Mode = ModeFlag.Markers,
                    X = x,
                    Y = y,
                    Marker = new Marker
                    {
                        Color = $"#009eda",
                        Size = 10
                    },
                    Meta = $"{trace.GetTraceId()}",
                    ShowLegend = false
                };

                if (!data.Any(d => d.As<Scatter>().Meta == newScatter.Meta))
                {
                    InvokeAsync(() => chart.AddTrace(newScatter));
                }

                result.Add(newScatter);
            }

            CalculateAverageDuration();
        });

        return result;
    }

    private void OnTracesUpdated(TracesRoot tracesRoot)
    {
        if (tracesRoot != null && tracesRoot.HasResource(ResourceName))
        {
            traces.AddRange(tracesRoot.ResourceSpans);
            this.StateHasChanged();
        }
    }

    private void CalculateAverageDuration()
    {
        if (traces.Where(t => t.GetTraceRoute().Contains(currentDataPoint.GetRouteName())).Count() == 0) 
            return;

        var avg = (decimal)traces.Where(t => t.GetTraceRoute().Contains(currentDataPoint.GetRouteName()))
            .Select(t => t.GetDuration().Milliseconds)
            .Average();

        averageDuration = (decimal)traces.Where(t => t.GetTraceRoute().Contains(currentDataPoint.GetRouteName()))
            .Select(t => t.GetDuration().Milliseconds)
            .Average() / 1000;

        minMaxDuration = new List<object>
        {
            (decimal)traces.Where(t => t.GetTraceRoute().Contains(currentDataPoint.GetRouteName()))
                .Select(t => t.GetDuration().Milliseconds)
                .Min() / 1000,
            (decimal)traces.Where(t => t.GetTraceRoute().Contains(currentDataPoint.GetRouteName()))
                .Select(t => t.GetDuration().Milliseconds)
                .Max() / 1000
        };

        averageDuration = Math.Truncate(averageDuration * 1000m) / 1000m;

        averageDurationIndicator.RefreshChart();
    }

    public void Dispose()
    {
        DataEvent -= UpdateData;
        cancellationToken?.Cancel();
    }
}

internal class DataEventArgs : EventArgs
{
    public DataEventArgs(IEnumerable<object> x, IEnumerable<object> y, int traceIndex)
    {
        X = x;
        Y = y;
        TraceIndex = traceIndex;
    }

    public IEnumerable<object> X { get; set; }
    public IEnumerable<object> Y { get; set; }
    public int TraceIndex { get; set; }
}

internal class DataJob
{
    public DataJob(DataEventArgs dataEventArgs, PlotlyChart chart, IList<ITrace> traces)
    {
        DataEventArgs = dataEventArgs;
        Chart = chart;
        Traces = traces;
    }

    public DataEventArgs DataEventArgs { get; set; }

    public PlotlyChart Chart { get; set; }

    public IList<ITrace> Traces { get; set; }

    public string Id
    {
        get
        {
            return $"{DataEventArgs.X.First().ToString()}_{DataEventArgs.Y.First().ToString()}";
        }
    }
}