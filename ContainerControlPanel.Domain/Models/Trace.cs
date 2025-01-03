using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ContainerControlPanel.Domain.Models;

public class TracesRoot
{
    [JsonPropertyName("resourceSpans")]
    public List<ResourceSpan> ResourceSpans { get; set; }

    public TracesRoot Clone()
    {
        return new TracesRoot
        {
            ResourceSpans = new List<ResourceSpan>(ResourceSpans)
        };
    }
}

public class Attribute
{
    [JsonPropertyName("key")]
    public string Key { get; set; }

    [JsonPropertyName("value")]
    public Value Value { get; set; }
}

public class Resource
{
    [JsonPropertyName("attributes")]
    public List<Attribute> Attributes { get; set; }
}

public class ResourceSpan
{
    [JsonPropertyName("resource")]
    public Resource Resource { get; set; }

    [JsonPropertyName("scopeSpans")]
    public List<ScopeSpan> ScopeSpans { get; set; }
}

public class Scope
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
}

public class ScopeSpan
{
    [JsonPropertyName("scope")]
    public Scope Scope { get; set; }

    [JsonPropertyName("spans")]
    public List<Span> Spans { get; set; }
}

public class Span
{
    [JsonPropertyName("traceId")]
    public string TraceId { get; set; }

    [JsonPropertyName("spanId")]
    public string SpanId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("kind")]
    public string Kind { get; set; }

    [JsonPropertyName("startTimeUnixNano")]
    public string StartTimeUnixNano { get; set; }

    [JsonPropertyName("endTimeUnixNano")]
    public string EndTimeUnixNano { get; set; }

    [JsonPropertyName("attributes")]
    public List<Attribute> Attributes { get; set; }
}

public class Value
{
    [JsonPropertyName("stringValue")]
    public string StringValue { get; set; }

    [JsonPropertyName("intValue")]
    public string IntValue { get; set; }
}

public class TraceListItemView
{
    public DateTime Timestamp { get; set; }
    public string Request { get; set; }
    public string TraceId { get; set; }
    public List<string> Source { get; set; }
    public TimeSpan Duration { get; set; }
}

public static class TracesExtensions
{
    public static List<ResourceSpan> GetTracesList(
        this List<TracesRoot> rootsList,
        bool orderDesc = true,
        bool routesOnly = false,
        int timeOffset = 0)
    {
        return rootsList
            .SelectMany(x => x.ResourceSpans)
            .Where(rs =>
                !routesOnly ||
                rs.ScopeSpans[0].Spans[0].Attributes.Any(a => a.Key == "url.path"))
            .OrderByDescending(rs => rs.ScopeSpans
                .SelectMany(ss => ss.Spans)
                .Min(span => long.Parse(span.StartTimeUnixNano)))
            .ToList();

    }

    public static DateTime GetTimestamp(this TracesRoot tracesRoot, int timeOffset)
        => tracesRoot.ResourceSpans[0].ScopeSpans[0].Spans[0].GetStartDate(timeOffset);

    public static string GetServiceName(this ResourceSpan resourceSpan)
        => resourceSpan.Resource.Attributes.Find(x => x.Key.Equals("service.name")).Value.StringValue;

    public static string GetTraceId(this ResourceSpan resourceSpan)
        => resourceSpan.ScopeSpans[0].Spans[0].TraceId;

    public static string GetTraceId(this TracesRoot tracesRoot)
        => tracesRoot.ResourceSpans[0].ScopeSpans[0].Spans[0].TraceId;

    public static string GetTraceRoute(this ResourceSpan resourceSpan)
        => resourceSpan.ScopeSpans?.Find(x => !x.Scope.Name.Equals("System.Net.Http"))?.Spans[0]?.Attributes?.Find(x => x.Key.Equals("url.path"))?.Value?.StringValue
            ?? resourceSpan.ScopeSpans?[0]?.Spans[0]?.Attributes?.Find(x => x.Key.Equals("url.path"))?.Value?.StringValue
            ?? "Unknown";

    public static string GetTraceName(this ResourceSpan resourceSpan)
        => resourceSpan.ScopeSpans[0].Spans[0].Name;

    public static TimeSpan GetDuration(this ResourceSpan resourceSpan)
    {
        var span = resourceSpan.ScopeSpans.Select(x => x.Spans[0]).OrderByDescending(s => s.GetDuration()).ToList();
        return span[0].GetDuration();
    }

    public static TimeSpan GetDuration(this Span span)
    {
        var startTime = decimal.Parse(span.StartTimeUnixNano);
        var endTime = decimal.Parse(span.EndTimeUnixNano);
        decimal startTimeMilliseconds = startTime / 1000000;
        decimal endTimeMilliseconds = endTime / 1000000;
        decimal durationMilliseconds = Math.Round(endTimeMilliseconds - startTimeMilliseconds, 2);
        return TimeSpan.FromMilliseconds((double)durationMilliseconds);
    }

    public static DateTime GetTimestamp(this ResourceSpan resourceSpan, int timeOffset)
    {
        var startTime = decimal.Parse(resourceSpan.ScopeSpans[0].Spans[0].StartTimeUnixNano);
        long milliseconds = Convert.ToInt64(Math.Round(startTime / 1000000));
        return DateTimeOffset.FromUnixTimeMilliseconds(milliseconds).AddHours(timeOffset).DateTime;
    }

    public static DateTime GetTimestamp(this Span span, int timeOffset)
    {
        var startTime = decimal.Parse(span.StartTimeUnixNano);
        long milliseconds = Convert.ToInt64(Math.Round(startTime / 1000000));
        return DateTimeOffset.FromUnixTimeMilliseconds(milliseconds).AddHours(timeOffset).DateTime;
    }

    public static DateTime GetStartDate(this Span span, int timeOffset)
    {
        var startTime = decimal.Parse(span.StartTimeUnixNano);
        long milliseconds = Convert.ToInt64(Math.Round(startTime / 1000000));
        return DateTimeOffset.FromUnixTimeMilliseconds(milliseconds).AddHours(timeOffset).DateTime;
    }

    public static DateTime GetEndDate(this Span span, int timeOffset)
    {
        var endTime = decimal.Parse(span.EndTimeUnixNano);
        long milliseconds = Convert.ToInt64(Math.Round(endTime / 1000000));
        return DateTimeOffset.FromUnixTimeMilliseconds(milliseconds).AddHours(timeOffset).DateTime;
    }

    public static List<string> GetResources(this List<TracesRoot> tracesRoot)
        => tracesRoot.SelectMany(r => r.ResourceSpans)
                     .Select(rs => rs.Resource?.Attributes?
                        .FirstOrDefault(a => a.Key == "service.name")?.Value?.StringValue)
                        .Where(serviceName => !string.IsNullOrEmpty(serviceName))
                     .Distinct()
                     .ToList();

    public static string GetAttributeValue(this Span span, string key)
        => span.Attributes.Find(x => x.Key.Equals(key))?.Value?.StringValue
            ?? (key == "http.route" ? "External" : string.Empty);

    public static List<(string Key, string Value)> GetAttributes(this Span span)
        => span.Attributes.Select(attr => (attr.Key, attr.Value.StringValue ?? attr.Value.IntValue)).ToList();

    public static bool HasResource(this TracesRoot tracesRoot, string resource)
        => tracesRoot.ResourceSpans.Any(rs => rs.Resource.Attributes
            .Any(attr => attr.Key == "service.name"
                && attr.Value?.StringValue == resource));

    public static int GetMatchAttemptsByTimestamp(this List<ResourceSpan> resourceSpans, DateTime dateTime, int timeOffset)
        => resourceSpans.Count(rs => rs.GetTimestamp(timeOffset) <= dateTime);

    public static List<Span> GetSpans(this TracesRoot tracesRoot)
        => tracesRoot.ResourceSpans[0].ScopeSpans.SelectMany(x => x.Spans).ToList();

    public static bool ContainsRoute(this ResourceSpan resourceSpan, string route)
        => resourceSpan.ScopeSpans.Any(x => x.Spans.Any(s => s.GetAttributeValue("http.route") == route));

    public static List<TraceListItemView> GroupTraces(this List<ResourceSpan> resourceSpans, int timeOffset)
    {
        List<TraceListItemView> traceList = new();

        foreach (var resourceSpan in resourceSpans)
        {
            var traceListItem = new TraceListItemView
            {
                Timestamp = resourceSpan.GetTimestamp(timeOffset),
                Request = resourceSpan.ScopeSpans[0].Spans[0].Name,
                TraceId = resourceSpan.GetTraceId(),
                Source = new List<string> { resourceSpan.GetServiceName() },
                Duration = resourceSpan.GetDuration()
            };

            if (traceList.Any(x => x.TraceId == traceListItem.TraceId))
            {
                var existingTrace = traceList.Find(x => x.TraceId == traceListItem.TraceId);
                existingTrace?.Source.AddRange(traceListItem.Source);
                if (existingTrace?.Duration < traceListItem.Duration)
                {
                    existingTrace.Duration = traceListItem.Duration;
                }
            }
            else
            {
                traceList.Add(traceListItem);
            }               
        }

        return traceList;
    }
}

