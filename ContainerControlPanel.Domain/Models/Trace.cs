using System.Text.Json.Serialization;

namespace ContainerControlPanel.Domain.Models;

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

public class Root
{
    [JsonPropertyName("resourceSpans")]
    public List<ResourceSpan> ResourceSpans { get; set; }
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

public static class Extensions
{
    public static string GetServiceName(this ResourceSpan resourceSpan)
        => resourceSpan.Resource.Attributes.Find(x => x.Key.Equals("service.name")).Value.StringValue;

    public static string GetTraceId(this ResourceSpan resourceSpan)
        => resourceSpan.ScopeSpans[0].Spans[0].TraceId;

    public static string GetTraceName(this ResourceSpan resourceSpan)
    {
        string method = resourceSpan.ScopeSpans[0].Spans[0].Attributes.Find(x => x.Key.Equals("http.request.method")).Value.StringValue;
        string route = resourceSpan.ScopeSpans[0].Spans[0].Attributes.Find(x => x.Key.Equals("url.path")).Value.StringValue;

        return $"{method} {route}";
    }

    public static TimeSpan GetDuration(this ResourceSpan resourceSpan)
    {
        var startTime = decimal.Parse(resourceSpan.ScopeSpans[0].Spans[0].StartTimeUnixNano);
        var endTime = decimal.Parse(resourceSpan.ScopeSpans[0].Spans[0].EndTimeUnixNano);

        long startTimeMilliseconds = Convert.ToInt64(Math.Round(startTime / 1000000));
        long endTimeMilliseconds = Convert.ToInt64(Math.Round(endTime / 1000000));

        return TimeSpan.FromMilliseconds(endTimeMilliseconds - startTimeMilliseconds);
    }

    public static DateTime GetTimestamp(this ResourceSpan resourceSpan)
    {
        var startTime = decimal.Parse(resourceSpan.ScopeSpans[0].Spans[0].StartTimeUnixNano);
        long milliseconds = Convert.ToInt64(Math.Round(startTime / 1000000));
        return DateTimeOffset.FromUnixTimeMilliseconds(milliseconds).DateTime;
    }
}

