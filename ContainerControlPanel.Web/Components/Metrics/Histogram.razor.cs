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

    private string? NetworkProtocolName
    {
        get => networkProtocolName;
        set
        {
            networkProtocolName = value;
            RefreshHistogram();
        }
    }

    private string? NetworkProtocolVersion
    {
        get => networkProtocolVersion;
        set
        {
            networkProtocolVersion = value;
            RefreshHistogram();
        }
    }

    private string? NetworkTransport
    {
        get => networkTransport;
        set
        {
            networkTransport = value;
            RefreshHistogram();
        }
    }

    private string? NetworkType
    {
        get => networkType;
        set
        {
            networkType = value;
            RefreshHistogram();
        }
    }

    private string? ServerAddress
    {
        get => serverAddress;
        set
        {
            serverAddress = value;
            RefreshHistogram();
        }
    }

    private string? ServerPort
    {
        get => serverPort;
        set
        {
            serverPort = value;
            RefreshHistogram();
        }
    }

    private string? TlsProtocolVersion
    {
        get => tlsProtocolVersion;
        set
        {
            tlsProtocolVersion = value;
            RefreshHistogram();
        }
    }

    private string? UrlScheme
    {
        get => urlScheme;
        set
        {
            urlScheme = value;
            RefreshHistogram();
        }
    }

    private string? ErrorType
    {
        get => errorType;
        set
        {
            errorType = value;
            RefreshHistogram();
        }
    }

    private string? NetworkPeerAddress
    {
        get => networkPeerAddress;
        set
        {
            networkPeerAddress = value;
            RefreshHistogram();
        }
    }

    private string? DnsQuestionName
    {
        get => dnsQuestionName;
        set
        {
            dnsQuestionName = value;
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

    private string? networkProtocolName = "all";

    private string? networkProtocolVersion = "all";

    private string? networkTransport = "all";

    private string? networkType = "all";

    private string? serverAddress = "all";

    private string? serverPort = "all";

    private string? tlsProtocolVersion = "all";

    private string? urlScheme = "all";

    private string? errorType = "all";

    private string? networkPeerAddress = "all";

    private string? dnsQuestionName = "all";

    private List<string> filterOptions = new();

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
        foreach (var attribute in Metrics.Histogram.DataPoints.First().Attributes)
        {
            filterOptions.Add(attribute.Key);
        }

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
            await WebSocketService.ConnectAsync($"ws://{Configuration["WebAPIHost"]}:{Configuration["WebAPIPort"]}/ws");
        }

        base.OnInitialized();
    }

    private void RefreshHistogram()
    {
        List<object> explicitBounds = Metrics.Histogram.DataPoints.First().ExplicitBounds.Select(x => (object)x).ToList();
        explicitBounds.Add($"{explicitBounds.Last()}+");
        object[] bucketCounts = new object[Metrics.Histogram.DataPoints.First().BucketCounts.Count()];

        foreach (var item in GetDataPoints())
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

    private List<DataPoint> GetDataPoints()
    {
        return Metrics.Histogram.DataPoints
            .Where(x => currentMethod == "all" || x.Attributes.Any(x => x.Key == "http.request.method" && x.Value.StringValue == currentMethod))
            .Where(x => currentStatus == "all" || x.Attributes.Any(x => x.Key == "http.response.status_code" && x.Value.IntValue == currentStatus))
            .Where(x => networkProtocolName == "all" || x.Attributes.Any(x => x.Key == "network.protocol.name" && x.Value.StringValue == networkProtocolName))
            .Where(x => networkProtocolVersion == "all" || x.Attributes.Any(x => x.Key == "network.protocol.version" && x.Value.StringValue == networkProtocolVersion))
            .Where(x => networkTransport == "all" || x.Attributes.Any(x => x.Key == "network.transport" && x.Value.StringValue == networkTransport))
            .Where(x => networkType == "all" || x.Attributes.Any(x => x.Key == "network.type" && x.Value.StringValue == networkType))
            .Where(x => serverAddress == "all" || x.Attributes.Any(x => x.Key == "server.address" && x.Value.StringValue == serverAddress))
            .Where(x => serverPort == "all" || x.Attributes.Any(x => x.Key == "server.port" && x.Value.IntValue == serverPort))
            .Where(x => tlsProtocolVersion == "all" || x.Attributes.Any(x => x.Key == "tls.protocol.version" && x.Value.StringValue == tlsProtocolVersion))
            .Where(x => urlScheme == "all" || x.Attributes.Any(x => x.Key == "url.scheme" && x.Value.StringValue == urlScheme))
            .Where(x => errorType == "all" || x.Attributes.Any(x => x.Key == "error.type" && x.Value.StringValue == errorType))
            .Where(x => networkPeerAddress == "all" || x.Attributes.Any(x => x.Key == "network.peer.address" && x.Value.StringValue == networkPeerAddress))
            .Where(x => dnsQuestionName == "all" || x.Attributes.Any(x => x.Key == "dns.question.name" && x.Value.StringValue == dnsQuestionName))
            .ToList();
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