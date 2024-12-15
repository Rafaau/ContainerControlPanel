using ContainerControlPanel.Domain.Models;
using ContainerControlPanel.Web.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System.Text;

namespace ContainerControlPanel.Web.Pages;

public partial class Trace(ITelemetryAPI telemetryAPI)
{
    [Inject]
    IStringLocalizer<Locales.Resource> Localizer { get; set; }

    [Inject]
    IConfiguration Configuration { get; set; }

    [Parameter]
    public string? TraceId { get; set; }

    private ITelemetryAPI telemetryAPI { get; set; } = telemetryAPI;

    private TracesRoot trace { get; set; }

    private List<Span> spans { get; set; }

    private string[] intervals { get; set; } = new string[5];

    private DateTime start { get; set; }

    private DateTime end { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await LoadTrace();

        base.OnInitialized();
    }

    private async Task LoadTrace()
    {
        trace = await telemetryAPI.GetTrace(TraceId);
        LoadSpans();
    }

    private void LoadSpans()
    {
        spans = trace.ResourceSpans
            .SelectMany(x => x.ScopeSpans.Select(x => x.Spans[0]))
            .OrderByDescending(s => s.GetDuration())
            .ToList();

        start = spans[0].GetStartDate();
        end = spans[0].GetEndDate();

        intervals[0] = "0ms";
        intervals[1] = $"{(spans[0].GetDuration().TotalMicroseconds / 1000 * 0.25).ToString("0.00")}ms";
        intervals[2] = $"{(spans[0].GetDuration().TotalMicroseconds / 1000 * 0.50).ToString("0.00")}ms";
        intervals[3] = $"{(spans[0].GetDuration().TotalMicroseconds / 1000 * 0.75).ToString("0.00")}ms";
        intervals[4] = $"{(spans[0].GetDuration().TotalMicroseconds / 1000).ToString("0.00")}ms";
    }

    private string GetHexString(string text)
    {
        byte[] byteArray = Encoding.Default.GetBytes(text);
        return BitConverter.ToString(byteArray).Replace("-", "").Substring(0, 6);
    }

    private string GetMarkerWidth(Span span)
    {
        var spanStart = span.GetStartDate();
        var spanEnd = span.GetEndDate();

        var duration = spanEnd - spanStart;
        var totalDuration = end - start;

        var width = (duration.TotalMilliseconds / totalDuration.TotalMilliseconds) * 401;

        return $"{width}%";
    }

    private string GetMarkerLeft(Span span)
    {
        var spanStart = span.GetStartDate();
        var totalDuration = end - start;
        var left = (spanStart - start).TotalMilliseconds / totalDuration.TotalMilliseconds * 100;
        return $"{left}%";
    }

    private string GetDurationSpanLeft(Span span)
    {
        var spanStart = span.GetStartDate();
        var spanEnd = span.GetEndDate();

        var duration = spanEnd - spanStart;
        var totalDuration = end - start;

        var width = (duration.TotalMilliseconds / totalDuration.TotalMilliseconds) * 401;
        var left = (spanStart - start).TotalMilliseconds / totalDuration.TotalMilliseconds * 100;

        return $"{left + width}%";
    }
}