using ContainerControlPanel.Web.Pages;
using Plotly.Blazor.Traces.IndicatorLib;
using Plotly.Blazor;
using Plotly.Blazor.LayoutLib;
using Plotly.Blazor.LayoutLib.GridLib;
using Plotly.Blazor.Traces;
using Microsoft.AspNetCore.Components;
using MudBlazor.Extensions;
using Plotly.Blazor.Traces.IndicatorLib.GaugeLib;

namespace ContainerControlPanel.Web.Components.Metrics;

public partial class IndicatorChart
{
    [Parameter]
    public decimal Value { get; set; }

    [Parameter]
    public decimal Reference { get; set; }

    [Parameter]
    public List<object> Range { get; set; }

    [Parameter]
    public string Suffix { get; set; }

    private PlotlyChart chart;
    private Config config;
    private Plotly.Blazor.Layout layout;
    private IList<ITrace> data;

    protected override void OnInitialized()
    {
        InitializeChart();

        base.OnInitialized();
    }

    public void RefreshChart()
    {
        data.First().As<Indicator>().Value = Value;
        data.First().As<Indicator>().Gauge.Axis.Range = Range;

        if (Value != 0 && Range != null)
        {
            data.First().As<Indicator>().Gauge.Bar.Color =
            Value < (decimal)Range[1] / 2
                ? "#009c1e"
                : Value > (decimal)Range[1] / 1.5m
                    ? "#b00000"
                    : "#dd6800";
        }
       
        chart.Update();
    }

    private void InitializeChart()
    {
        config = new Config
        {
            Responsive = true,
            DisplayLogo = false,
            DisplayModeBar = Plotly.Blazor.ConfigLib.DisplayModeBarEnum.False
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
                L = 20,
                R = 40,
                B = 0,
                T = 0
            },
            Width = 268,
            Height = 280
        };

        data = new List<ITrace>
        {
            new Indicator
            {
                Mode = ModeFlag.Number | ModeFlag.Delta | ModeFlag.Gauge,
                Value = Value,       
                Number = new Number
                {
                    Suffix = Suffix
                },
                Delta = new Delta
                {
                    Reference = Reference,
                    Relative = true,
                    Decreasing = new Plotly.Blazor.Traces.IndicatorLib.DeltaLib.Decreasing
                    {
                        Color = "#009c1e"
                    },
                    Increasing = new Plotly.Blazor.Traces.IndicatorLib.DeltaLib.Increasing
                    {
                        Color = "#b00000"
                    }
                },
                Gauge = new Gauge
                {
                    Bar = new Plotly.Blazor.Traces.IndicatorLib.GaugeLib.Bar
                    {
                        Color = "#ffffff"
                    },
                    Axis = new Axis
                    {
                        Range = new List<object> { 0, 1 },
                    }
                }
            }
        };
    }
}