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

public partial class MatchAttempts(ITelemetryAPI telemetryAPI) : IDisposable
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

    private List<ResourceSpan> traces { get; set; }

    private string? currentDataPoint { get; set; } = "all";

    private List<ResourceSpan> filteredTraces
    {
        get
        {
            if (currentDataPoint == "all")
            {
                return traces;
            }
            return traces.Where(x => x.ContainsRoute(currentDataPoint)).ToList();
        }
    }

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
        await GetTraces();

        if (bool.Parse(Configuration["Realtime"]))
        {
            WebSocketService.TracesUpdated += OnTracesUpdated;
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
                    DateTime.Now.AddMinutes(-timestamp * 0.8),
                    DateTime.Now.AddMinutes(-timestamp * 0.7),
                    DateTime.Now.AddMinutes(-timestamp * 0.6),
                    DateTime.Now.AddMinutes(-timestamp * 0.5),
                    DateTime.Now.AddMinutes(-timestamp * 0.4),
                    DateTime.Now.AddMinutes(-timestamp * 0.3),
                    DateTime.Now.AddMinutes(-timestamp * 0.2),
                    DateTime.Now.AddMinutes(-timestamp * 0.1),
                    DateTime.Now
                };

                var may = new List<object>
                {
                    filteredTraces?.GetMatchAttemptsByTimestamp(DateTime.Now.AddMinutes(-timestamp), int.Parse(Configuration["TimeOffset"])) ?? 0,
                    filteredTraces?.GetMatchAttemptsByTimestamp(DateTime.Now.AddMinutes(-timestamp * 0.9), int.Parse(Configuration["TimeOffset"])) ?? 0,
                    filteredTraces?.GetMatchAttemptsByTimestamp(DateTime.Now.AddMinutes(-timestamp * 0.8), int.Parse(Configuration["TimeOffset"])) ?? 0,
                    filteredTraces?.GetMatchAttemptsByTimestamp(DateTime.Now.AddMinutes(-timestamp * 0.7), int.Parse(Configuration["TimeOffset"])) ?? 0,
                    filteredTraces?.GetMatchAttemptsByTimestamp(DateTime.Now.AddMinutes(-timestamp * 0.6), int.Parse(Configuration["TimeOffset"])) ?? 0,
                    filteredTraces?.GetMatchAttemptsByTimestamp(DateTime.Now.AddMinutes(-timestamp * 0.5), int.Parse(Configuration["TimeOffset"])) ?? 0,
                    filteredTraces?.GetMatchAttemptsByTimestamp(DateTime.Now.AddMinutes(-timestamp * 0.4), int.Parse(Configuration["TimeOffset"])) ?? 0,
                    filteredTraces?.GetMatchAttemptsByTimestamp(DateTime.Now.AddMinutes(-timestamp * 0.3), int.Parse(Configuration["TimeOffset"])) ?? 0,
                    filteredTraces?.GetMatchAttemptsByTimestamp(DateTime.Now.AddMinutes(-timestamp * 0.2), int.Parse(Configuration["TimeOffset"])) ?? 0,
                    filteredTraces?.GetMatchAttemptsByTimestamp(DateTime.Now.AddMinutes(-timestamp * 0.1), int.Parse(Configuration["TimeOffset"])) ?? 0,
                    filteredTraces?.Count() ?? 0
                };

                DataEvent?.Invoke(this, new DataJob(new DataEventArgs(max, may, 0), chart, data));
             
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
    
    private void OnTracesUpdated(TracesRoot tracesRoot)
    {
        if (tracesRoot != null && tracesRoot.HasResource(ResourceName))
        {
            traces.AddRange(tracesRoot.ResourceSpans);
            this.StateHasChanged();
        }
    }

    public void Dispose()
    {
        DataEvent -= UpdateData;
        cancellationToken?.Cancel();
    }
}