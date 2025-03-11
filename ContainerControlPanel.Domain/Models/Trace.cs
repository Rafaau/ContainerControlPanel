using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace ContainerControlPanel.Domain.Models;

public class Trace
{
    /// <summary>
    /// Gets or sets the ID of the trace
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets the DateTime when the trace was created
    /// </summary>
    [BsonElement("createdAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// Gets or sets the timestamp
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the request
    /// </summary>
    public string Request { get; set; }

    /// <summary>
    /// Gets or sets the source
    /// </summary>
    public List<string> Source { get; set; }

    /// <summary>
    /// Gets or sets the duration
    /// </summary>
    public TimeSpan Duration { get; set; }
}

/// <summary>
/// Class to represent a trace output
/// </summary>
public class TracesRoot
{
    /// <summary>
    /// Gets or sets the ID of the trace
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets the resource spans
    /// </summary>
    [JsonPropertyName("resourceSpans")]
    public List<ResourceSpan> ResourceSpans { get; set; }

    /// <summary>
    /// Gets or sets the DateTime when the trace was created
    /// </summary>
    [BsonElement("createdAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// Gets or sets the resource name
    /// </summary>
    public string ResourceName { get; set; }

    /// <summary>
    /// Clones the current instance
    /// </summary>
    /// <returns>Returns a new instance of <see cref="TracesRoot"/></returns>
    public TracesRoot Clone()
    {
        return new TracesRoot
        {
            ResourceSpans = new List<ResourceSpan>(ResourceSpans)
        };
    }
}

/// <summary>
/// Class to represent an attribute
/// </summary>
public class Attribute
{
    /// <summary>
    /// Gets or sets the key of the attribute
    /// </summary>
    [JsonPropertyName("key")]
    public string Key { get; set; }

    /// <summary>
    /// Gets or sets the value of the attribute
    /// </summary>
    [JsonPropertyName("value")]
    public Value Value { get; set; }
}

/// <summary>
/// Class to represent a resource
/// </summary>
public class Resource
{
    /// <summary>
    /// Gets or sets the attributes of the resource
    /// </summary>
    [JsonPropertyName("attributes")]
    public List<Attribute> Attributes { get; set; }
}

/// <summary>
/// Class to represent a resource span
/// </summary>
public class ResourceSpan
{
    /// <summary>
    /// Gets or sets the resource
    /// </summary>
    [JsonPropertyName("resource")]
    public Resource Resource { get; set; }

    /// <summary>
    /// Gets or sets the scope spans
    /// </summary>
    [JsonPropertyName("scopeSpans")]
    public List<ScopeSpan> ScopeSpans { get; set; }
}

/// <summary>
/// Class to represent a scope
/// </summary>
public class Scope
{
    /// <summary>
    /// Gets or sets the name of the scope
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }
}

/// <summary>
/// Class to represent a scope span
/// </summary>
public class ScopeSpan
{
    /// <summary>
    /// Gets or sets the scope
    /// </summary>
    [JsonPropertyName("scope")]
    public Scope Scope { get; set; }

    /// <summary>
    /// Gets or sets the spans
    /// </summary>
    [JsonPropertyName("spans")]
    public List<Span> Spans { get; set; }
}

/// <summary>
/// Class to represent a span
/// </summary>
public class Span
{
    /// <summary>
    /// Gets or sets the trace ID
    /// </summary>
    [JsonPropertyName("traceId")]
    public string TraceId { get; set; }

    /// <summary>
    /// Gets or sets the span ID
    /// </summary>
    [JsonPropertyName("spanId")]
    public string SpanId { get; set; }

    /// <summary>
    /// Gets or sets the name of the span
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the kind of the span
    /// </summary>
    [JsonPropertyName("kind")]
    public string Kind { get; set; }

    /// <summary>
    /// Gets or sets the start time in Unix nanoseconds
    /// </summary>
    [JsonPropertyName("startTimeUnixNano")]
    public string StartTimeUnixNano { get; set; }

    /// <summary>
    /// Gets or sets the end time in Unix nanoseconds
    /// </summary>
    [JsonPropertyName("endTimeUnixNano")]
    public string EndTimeUnixNano { get; set; }

    /// <summary>
    /// Gets or sets the attributes of the span
    /// </summary>
    [JsonPropertyName("attributes")]
    public List<Attribute> Attributes { get; set; }
}

/// <summary>
/// Class to represent a value
/// </summary>
public class Value
{
    /// <summary>
    /// Gets or sets the string value
    /// </summary>
    [JsonPropertyName("stringValue")]
    public string StringValue { get; set; }

    /// <summary>
    /// Gets or sets the integer value
    /// </summary>
    [JsonPropertyName("intValue")]
    public string IntValue { get; set; }
}

/// <summary>
/// Class to represent a trace list item view
/// </summary>
public class TraceListItemView
{
    /// <summary>
    /// Gets or sets the timestamp
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the request
    /// </summary>
    public string Request { get; set; }

    /// <summary>
    /// Gets or sets the trace ID
    /// </summary>
    public string TraceId { get; set; }

    /// <summary>
    /// Gets or sets the source
    /// </summary>
    public List<string> Source { get; set; }

    /// <summary>
    /// Gets or sets the duration
    /// </summary>
    public TimeSpan Duration { get; set; }
}

/// <summary>
/// Extension methods for <see cref="TracesRoot"/>
/// </summary>
public static class TracesExtensions
{
    /// <summary>
    /// Gets the traces list
    /// </summary>
    /// <param name="rootsList">List of traces roots</param>
    /// <param name="orderDesc">Order the list in descending order</param>
    /// <param name="routesOnly">Return only routes</param>
    /// <param name="timeOffset">Time offset</param>
    /// <returns>Returns a list of <see cref="ResourceSpan"/></returns>
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

    /// <summary>
    /// Gets the timestamp of the trace
    /// </summary>
    /// <param name="tracesRoot">Traces root object</param>
    /// <param name="timeOffset">Time offset</param>
    /// <returns>Returns the timestamp of the trace</returns>
    public static DateTime GetTimestamp(this TracesRoot tracesRoot, int timeOffset)
        => tracesRoot.ResourceSpans[0].ScopeSpans[0].Spans[0].GetStartDate(timeOffset);

    /// <summary>
    /// Gets the service name
    /// </summary>
    /// <param name="resourceSpan">Resource span object</param>
    /// <returns>Returns the service name</returns>
    public static string GetServiceName(this ResourceSpan resourceSpan)
        => resourceSpan.Resource.Attributes.Find(x => x.Key.Equals("service.name")).Value.StringValue;

    /// <summary>
    /// Gets the trace ID
    /// </summary>
    /// <param name="resourceSpan">Resource span object</param>
    /// <returns>Returns the trace ID</returns>
    public static string GetTraceId(this ResourceSpan resourceSpan)
        => resourceSpan.ScopeSpans[0].Spans[0].TraceId;

    /// <summary>
    /// Gets the trace ID
    /// </summary>
    /// <param name="tracesRoot">Traces root object</param>
    /// <returns>Returns the trace ID</returns>
    public static string GetTraceId(this TracesRoot tracesRoot)
        => tracesRoot.ResourceSpans[0].ScopeSpans[0].Spans[0].TraceId;

    /// <summary>
    /// Gets the trace route
    /// </summary>
    /// <param name="resourceSpan">Resource span object</param>
    /// <returns>Returns the trace route</returns>
    public static string GetTraceRoute(this ResourceSpan resourceSpan)
        => resourceSpan.ScopeSpans?.Find(x => !x.Scope.Name.Equals("System.Net.Http"))?.Spans[0]?.Attributes?.Find(x => x.Key.Equals("url.path"))?.Value?.StringValue
            ?? resourceSpan.ScopeSpans?[0]?.Spans[0]?.Attributes?.Find(x => x.Key.Equals("url.path"))?.Value?.StringValue
            ?? "Unknown";

    /// <summary>
    /// Gets the trace name
    /// </summary>
    /// <param name="resourceSpan">Resource span object</param>
    /// <returns>Returns the trace name</returns>
    public static string GetTraceName(this ResourceSpan resourceSpan)
        => resourceSpan.ScopeSpans[0].Spans[0].Name;

    /// <summary>
    /// Gets the duration of the resource span
    /// </summary>
    /// <param name="resourceSpan">Resource span object</param>
    /// <returns>Returns the duration of the resource span</returns>
    public static TimeSpan GetDuration(this ResourceSpan resourceSpan)
    {
        var span = resourceSpan.ScopeSpans.Select(x => x.Spans[0]).OrderByDescending(s => s.GetDuration()).ToList();
        return span[0].GetDuration();
    }

    /// <summary>
    /// Gets the duration of the span
    /// </summary>
    /// <param name="span">Span object</param>
    /// <returns>Returns the duration of the span</returns>
    public static TimeSpan GetDuration(this Span span)
    {
        var startTime = decimal.Parse(span.StartTimeUnixNano);
        var endTime = decimal.Parse(span.EndTimeUnixNano);
        decimal startTimeMilliseconds = startTime / 1000000;
        decimal endTimeMilliseconds = endTime / 1000000;
        decimal durationMilliseconds = Math.Round(endTimeMilliseconds - startTimeMilliseconds, 2);
        return TimeSpan.FromMilliseconds((double)durationMilliseconds);
    }

    /// <summary>
    /// Gets the timestamp of the resource span
    /// </summary>
    /// <param name="resourceSpan">Resource span object</param>
    /// <param name="timeOffset">Time offset</param>
    /// <returns>Returns the timestamp of the resource span</returns>
    public static DateTime GetTimestamp(this ResourceSpan resourceSpan, int timeOffset)
    {
        var startTime = decimal.Parse(resourceSpan.ScopeSpans[0].Spans[0].StartTimeUnixNano);
        long milliseconds = Convert.ToInt64(Math.Round(startTime / 1000000));
        return DateTimeOffset.FromUnixTimeMilliseconds(milliseconds).AddHours(timeOffset).DateTime;
    }

    /// <summary>
    /// Gets the timestamp of the span
    /// </summary>
    /// <param name="span">Span object</param>
    /// <param name="timeOffset">Time offset</param>
    /// <returns>Returns the timestamp of the span</returns>
    public static DateTime GetTimestamp(this Span span, int timeOffset)
    {
        var startTime = decimal.Parse(span.StartTimeUnixNano);
        long milliseconds = Convert.ToInt64(Math.Round(startTime / 1000000));
        return DateTimeOffset.FromUnixTimeMilliseconds(milliseconds).AddHours(timeOffset).DateTime;
    }

    /// <summary>
    /// Gets the start date of the span
    /// </summary>
    /// <param name="span">Span object</param>
    /// <param name="timeOffset">Time offset</param>
    /// <returns>Returns the start date of the span</returns>
    public static DateTime GetStartDate(this Span span, int timeOffset)
    {
        var startTime = decimal.Parse(span.StartTimeUnixNano);
        long milliseconds = Convert.ToInt64(Math.Round(startTime / 1000000));
        return DateTimeOffset.FromUnixTimeMilliseconds(milliseconds).AddHours(timeOffset).DateTime;
    }

    /// <summary>
    /// Gets the end date of the span
    /// </summary>
    /// <param name="span">Span object</param>
    /// <param name="timeOffset">Time offset</param>
    /// <returns>Returns the end date of the span</returns>
    public static DateTime GetEndDate(this Span span, int timeOffset)
    {
        var endTime = decimal.Parse(span.EndTimeUnixNano);
        long milliseconds = Convert.ToInt64(Math.Round(endTime / 1000000));
        return DateTimeOffset.FromUnixTimeMilliseconds(milliseconds).AddHours(timeOffset).DateTime;
    }

    /// <summary>
    /// Gets the service name
    /// </summary>
    /// <param name="tracesRoot">Traces root object</param>
    /// <returns>Returns the service name</returns>
    public static List<string> GetResources(this List<TracesRoot> tracesRoot)
        => tracesRoot.SelectMany(r => r.ResourceSpans)
                     .Select(rs => rs.Resource?.Attributes?
                        .FirstOrDefault(a => a.Key == "service.name")?.Value?.StringValue)
                        .Where(serviceName => !string.IsNullOrEmpty(serviceName))
                     .Distinct()
                     .ToList();

    /// <summary>
    /// Gets the service name
    /// </summary>
    /// <param name="span">Span object</param>
    /// <param name="key">Key of the attribute</param>
    /// <returns>Returns the value of the attribute</returns>
    public static string GetAttributeValue(this Span span, string key)
        => span.Attributes.Find(x => x.Key.Equals(key))?.Value?.StringValue
            ?? (key == "http.route" ? "External" : string.Empty);

    /// <summary>
    /// Gets the attributes of the span
    /// </summary>
    /// <param name="span">Span object</param>
    /// <returns>Returns a list of attributes</returns>
    public static List<(string Key, string Value)> GetAttributes(this Span span)
        => span.Attributes.Select(attr => (attr.Key, attr.Value.StringValue ?? attr.Value.IntValue)).ToList();

    /// <summary>
    /// Checks if the traces root has a resource
    /// </summary>
    /// <param name="tracesRoot">Traces root object</param>
    /// <param name="resource">Resource name</param>
    /// <returns>Returns a boolean value indicating whether the traces root has the resource</returns>
    public static bool HasResource(this TracesRoot tracesRoot, string resource)
        => tracesRoot.ResourceSpans.Any(rs => rs.Resource.Attributes
            .Any(attr => attr.Key == "service.name"
                && attr.Value?.StringValue == resource));

    public static string GetResourceName(this TracesRoot tracesRoot)
        => tracesRoot.ResourceSpans[0].Resource.Attributes.Find(x => x.Key == "service.name").Value.StringValue;

    /// <summary>
    /// Gets the trace route
    /// </summary>
    /// <param name="resourceSpans">List of resource spans</param>
    /// <param name="dateTime">Date time</param>
    /// <param name="timeOffset">Time offset</param>
    /// <returns>Returns the trace route</returns>
    public static int GetMatchAttemptsByTimestamp(this List<ResourceSpan> resourceSpans, DateTime dateTime, int timeOffset)
        => resourceSpans.Count(rs => rs.GetTimestamp(timeOffset) <= dateTime);

    /// <summary>
    /// Gets the trace route
    /// </summary>
    /// <param name="tracesRoot">Traces root object</param>
    /// <returns>Returns the trace route</returns>
    public static List<Span> GetSpans(this TracesRoot tracesRoot)
        => tracesRoot.ResourceSpans[0].ScopeSpans.SelectMany(x => x.Spans).ToList();

    /// <summary>
    /// Gets the trace route
    /// </summary>
    /// <param name="resourceSpan">Resource span object</param>
    /// <param name="route">Route of the span</param>
    /// <returns>Returns a boolean value indicating whether the resource span contains the route</returns>
    public static bool ContainsRoute(this ResourceSpan resourceSpan, string route)
        => resourceSpan.ScopeSpans.Any(x => x.Spans.Any(s => s.GetAttributeValue("http.route") == route));

    /// <summary>
    /// Groups the traces
    /// </summary>
    /// <param name="resourceSpans">List of resource spans</param>
    /// <param name="timeOffset">Time offset</param>
    /// <returns>Returns a list of <see cref="TraceListItemView"/></returns>
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

