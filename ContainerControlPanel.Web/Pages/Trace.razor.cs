using ContainerControlPanel.Domain.Models;
using ContainerControlPanel.Web.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using MudBlazor;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ContainerControlPanel.Web.Pages;

public partial class Trace(ITelemetryAPI telemetryAPI)
{
    [Inject]
    IStringLocalizer<Locales.Resource> Localizer { get; set; }

    [Inject]
    IConfiguration Configuration { get; set; }

    [Inject]
    IJSRuntime JS { get; set; } 

    [Parameter]
    public string? TraceId { get; set; }

    private ITelemetryAPI telemetryAPI { get; set; } = telemetryAPI;

    private List<TracesRoot> traceRoots { get; set; } = new();

    private List<Span> spans { get; set; }

    private string[] intervals { get; set; } = new string[5];

    private DateTime start { get; set; }

    private DateTime end { get; set; }

    private Span currentSpan { get; set; }

    private RequestResponse? requestResponse { get; set; } = null;

    private RequestTab requestTab { get; set; } = RequestTab.Headers;

    private string formattedRequestJson { get; set; }

    private string formattedResponseJson { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await LoadTrace();
        await LoadRequestAndResponse();

        base.OnInitialized();
    }

    private async Task LoadTrace()
    {
        traceRoots = await telemetryAPI.GetTrace(TraceId);
        LoadSpans();
    }

    private async Task LoadRequestAndResponse()
    {
        var result = await telemetryAPI.GetRequestAndResponse(TraceId);

        if (result.IsSuccessStatusCode)
        {
            requestResponse = result.Content;

            if (requestResponse?.Request?.Body != null)
            {
                var json = JsonSerializer.Serialize(requestResponse.Request.Body, new JsonSerializerOptions { WriteIndented = true });
                formattedRequestJson = await JS.InvokeAsync<string>("colorizeJson", json);
            }
            
            if (requestResponse?.Response != null)
            {
                var preJson = JsonObject.Parse(requestResponse.Response);
                var json = JsonSerializer.Serialize(preJson, new JsonSerializerOptions { WriteIndented = true });
                formattedResponseJson = await JS.InvokeAsync<string>("colorizeJson", json);
            }
        }
    }

    private void LoadSpans()
    {
        spans = traceRoots
            .SelectMany(x => x.ResourceSpans.SelectMany(x => x.ScopeSpans.Select(x => x.Spans[0])))
            .OrderByDescending(s => s.GetDuration())
            .ToList();

        currentSpan = spans[0];
        start = spans[0].GetStartDate(int.Parse(Configuration["TimeOffset"]));
        end = spans[0].GetEndDate(int.Parse(Configuration["TimeOffset"]));

        intervals[0] = new TimeSpan(0).FormatDuration();
        intervals[1] = (spans[0].GetDuration() * 0.25).FormatDuration();
        intervals[2] = (spans[0].GetDuration() * 0.50).FormatDuration();
        intervals[3] = (spans[0].GetDuration() * 0.75).FormatDuration();
        intervals[4] = spans[0].GetDuration().FormatDuration();
    }

    private string GetHexString(string text)
    {
        byte[] byteArray = Encoding.Default.GetBytes(text);
        return BitConverter.ToString(byteArray).Replace("-", "").Substring(0, 6);
    }

    private string GetMarkerWidth(Span span)
    {
        var spanStart = span.GetStartDate(int.Parse(Configuration["TimeOffset"]));
        var spanEnd = span.GetEndDate(int.Parse(Configuration["TimeOffset"]));

        var duration = spanEnd - spanStart;
        var totalDuration = end - start;

        var width = (duration.TotalMilliseconds / totalDuration.TotalMilliseconds) * 400;

        return $"{width}%";
    }

    private string GetMarkerLeft(Span span)
    {
        var spanStart = span.GetStartDate(int.Parse(Configuration["TimeOffset"]));
        var totalDuration = end - start;
        var left = (spanStart - start).TotalMilliseconds / totalDuration.TotalMilliseconds * 400;
        return $"{left}%";
    }

    private string GetDurationSpanLeft(Span span)
    {
        var spanStart = span.GetStartDate(int.Parse(Configuration["TimeOffset"]));
        var spanEnd = span.GetEndDate(int.Parse(Configuration["TimeOffset"]));

        var duration = spanEnd - spanStart;
        var totalDuration = end - start;

        var width = (duration.TotalMilliseconds / totalDuration.TotalMilliseconds) * 400;
        var left = (spanStart - start).TotalMilliseconds / totalDuration.TotalMilliseconds * 400;

        return $"{left + width}%";
    }

    private async Task CopyToClipboard(string text)
    {
        await JS.InvokeVoidAsync("navigator.clipboard.writeText", text);
        Snackbar.Clear();
        Snackbar.Configuration.PositionClass = Defaults.Classes.Position.BottomRight;
        Snackbar.Add(Localizer[Locales.Resource.Copied], Severity.Normal);
    }
}

public enum RequestTab
{
    Headers,
    Params,
    Body
}

public static class TraceExtensions
{
    public static string FormatDuration(this TimeSpan duration, bool format = true)
    {
        if (duration == TimeSpan.Zero) return "0";

        return duration.TotalMilliseconds switch
        {
            < 1000 => $"{duration.TotalMilliseconds.ToString(format ? "0.000" : "0")}ms",
            < 60000 => $"{duration.TotalSeconds.ToString(format ? "0.00" : "0")}s",
            < 3600000 => $"{duration.TotalMinutes.ToString(format ? "0.00" : "0")}m",
            _ => $"{duration.TotalHours.ToString(format ? "0.00" : "0")}h"
        };
    }
}