using System.Text.Json.Serialization;

namespace ContainerControlPanel.Domain.Models;

public class TracesRoot
{
    [JsonPropertyName("resourceSpans")]
    public List<ResourceSpan> ResourceSpans { get; set; }
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

public static class TracesExtensions
{
    public static List<ResourceSpan> GetTracesList(
        this List<TracesRoot> rootsList,
        bool orderDesc = true,
        string filter = "all",
        bool routesOnly = false)
    {
        return rootsList
            .SelectMany(x => x.ResourceSpans)
            .Where(rs =>
                filter == "all" ||
                rs.Resource.Attributes
                    .Any(attr => attr.Key == "service.name"
                        && attr.Value?.StringValue == filter))
            .Where(rs =>
                !routesOnly ||
                rs.ScopeSpans[0].Spans[0].Attributes.Any(a => a.Key == "url.path"))
            .OrderByDescending(rs => rs.ScopeSpans
                .SelectMany(ss => ss.Spans)
                .Min(span => long.Parse(span.StartTimeUnixNano)))
            .ToList();

    }

    public static string GetServiceName(this ResourceSpan resourceSpan)
        => resourceSpan.Resource.Attributes.Find(x => x.Key.Equals("service.name")).Value.StringValue;

    public static string GetTraceId(this ResourceSpan resourceSpan)
        => resourceSpan.ScopeSpans[0].Spans[0].TraceId;

    public static string GetTraceId(this TracesRoot tracesRoot)
        => tracesRoot.ResourceSpans[0].ScopeSpans[0].Spans[0].TraceId;

    public static string GetTraceRoute(this ResourceSpan resourceSpan)
        => resourceSpan.ScopeSpans.Find(x => !x.Scope.Name.Equals("System.Net.Http")).Spans[0].Attributes.Find(x => x.Key.Equals("url.path"))?.Value.StringValue
            ?? resourceSpan.ScopeSpans[0].Spans[0].Attributes.Find(x => x.Key.Equals("url.path"))?.Value.StringValue;

    public static string GetTraceName(this ResourceSpan resourceSpan)
        => resourceSpan.ScopeSpans.Find(x => !x.Scope.Name.Equals("System.Net.Http")).Spans[0].Name
            ?? resourceSpan.ScopeSpans[0].Spans[0].Name;

    public static TimeSpan GetDuration(this ResourceSpan resourceSpan)
    {
        var startTime = decimal.Parse(resourceSpan.ScopeSpans[0].Spans[0].StartTimeUnixNano);
        var endTime = decimal.Parse(resourceSpan.ScopeSpans[0].Spans[0].EndTimeUnixNano);
        long startTimeMilliseconds = Convert.ToInt64(Math.Round(startTime / 1000000));
        long endTimeMilliseconds = Convert.ToInt64(Math.Round(endTime / 1000000));
        return TimeSpan.FromMilliseconds(endTimeMilliseconds - startTimeMilliseconds);
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

    public static DateTime GetStartDate(this Span span)
    {
        var startTime = decimal.Parse(span.StartTimeUnixNano);
        long milliseconds = Convert.ToInt64(Math.Round(startTime / 1000000));
        return DateTimeOffset.FromUnixTimeMilliseconds(milliseconds).DateTime;
    }

    public static DateTime GetEndDate(this Span span)
    {
        var endTime = decimal.Parse(span.EndTimeUnixNano);
        long milliseconds = Convert.ToInt64(Math.Round(endTime / 1000000));
        return DateTimeOffset.FromUnixTimeMilliseconds(milliseconds).DateTime;
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
            ?? (key == "http.route" ? "Browser Link" : string.Empty);

    public static bool HasResource(this TracesRoot tracesRoot, string resource)
        => tracesRoot.ResourceSpans.Any(rs => rs.Resource.Attributes
            .Any(attr => attr.Key == "service.name"
                && attr.Value?.StringValue == resource));
}

