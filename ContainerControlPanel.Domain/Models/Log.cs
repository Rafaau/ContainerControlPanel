using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace ContainerControlPanel.Domain.Models;

public class Log
{
    /// <summary>
    /// Gets or sets the ID of the log
    /// </summary>
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the resource
    /// </summary>
    public string ResourceName { get; set; }

    /// <summary>
    /// Gets or sets the severity of the log
    /// </summary>
    public string Severity { get; set; }

    /// <summary>
    /// Gets or sets the message of the log
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Gets or sets the DateTime of the log creation
    /// </summary>
    [BsonElement("createdAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public string Scope { get; set; }

    public string ServiceInstance { get; set; }

    public string TraceId { get; set; }

    public string SpanId { get; set; }

    public int Flags { get; set; }

    public string TelemetrySdkName { get; set; }

    public string TelemetrySdkLanguage { get; set; }

    public string TelemetrySdkVersion { get; set; }
}

/// <summary>
/// Class to represent the logs output
/// </summary>
public class LogsRoot
{
    /// <summary>
    /// Gets or sets the ID of the log
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets the resource logs
    /// </summary>
    [JsonPropertyName("resourceLogs")]
    public List<ResourceLog> ResourceLogs { get; set; }

    /// <summary>
    /// Gets or sets the DateTime of the log creation
    /// </summary>
    [BsonElement("createdAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// Gets or sets the name of the resource
    /// </summary>
    public string ResourceName { get; set; }

    /// <summary>
    /// Gets or sets the severity of the log
    /// </summary>
    public string Severity { get; set; }

    /// <summary>
    /// Gets or sets the message of the log
    /// </summary>
    public string Message { get; set; }
}

/// <summary>
/// Class to represent the body of a log
/// </summary>
public class Body
{
    /// <summary>
    /// Gets or sets the string value
    /// </summary>
    [JsonPropertyName("stringValue")]
    public string StringValue { get; set; }
}

/// <summary>
/// Class to represent a log record
/// </summary>
public class LogRecord
{
    /// <summary>
    /// Gets or sets the time in Unix nano format
    /// </summary>
    [JsonPropertyName("timeUnixNano")]
    public string TimeUnixNano { get; set; }

    /// <summary>
    /// Gets or sets the severity number
    /// </summary>
    [JsonPropertyName("severityNumber")]
    public string SeverityNumber { get; set; }

    /// <summary>
    /// Gets or sets the severity text
    /// </summary>
    [JsonPropertyName("severityText")]
    public string SeverityText { get; set; }

    /// <summary>
    /// Gets or sets the body
    /// </summary>
    [JsonPropertyName("body")]
    public Body Body { get; set; }

    /// <summary>
    /// Gets or sets the flags
    /// </summary>
    [JsonPropertyName("flags")]
    public int Flags { get; set; }

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
    /// Gets or sets the observed time in Unix nano format
    /// </summary>
    [JsonPropertyName("observedTimeUnixNano")]
    public string ObservedTimeUnixNano { get; set; }
}

/// <summary>
/// Class to represent a resource log
/// </summary>
public class ResourceLog
{
    /// <summary>
    /// Gets or sets the resource
    /// </summary>
    [JsonPropertyName("resource")]
    public Resource Resource { get; set; }

    /// <summary>
    /// Gets or sets the scope logs
    /// </summary>
    [JsonPropertyName("scopeLogs")]
    public List<ScopeLog> ScopeLogs { get; set; }
}

/// <summary>
/// Class to represent a scope log
/// </summary>
public class ScopeLog
{
    /// <summary>
    /// Gets or sets the scope
    /// </summary>
    [JsonPropertyName("scope")]
    public Scope Scope { get; set; }

    /// <summary>
    /// Gets or sets the log records
    /// </summary>
    [JsonPropertyName("logRecords")]
    public List<LogRecord> LogRecords { get; set; }
}

/// <summary>
/// Class to represent a log view
/// </summary>
public class LogView
{
    /// <summary>
    /// Gets or sets the resource name
    /// </summary>
    public string? ResourceName { get; set; }

    /// <summary>
    /// Gets or sets the severity
    /// </summary>
    public string? Severity { get; set; }

    /// <summary>
    /// Gets or sets the timestamp
    /// </summary>
    public DateTime? Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the message
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Gets or sets the trace ID
    /// </summary>
    public string? TraceId { get; set; }
}

/// <summary>
/// Class to represent the log details view
/// </summary>
public class LogDetailsView
{
    /// <summary>
    /// Gets or sets the resource name
    /// </summary>
    public string? ResourceName { get; set; }

    /// <summary>
    /// Gets or sets the severity
    /// </summary>
    public string? Severity { get; set; }

    /// <summary>
    /// Gets or sets the timestamp
    /// </summary>
    public DateTime? Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the message
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Gets or sets the trace ID
    /// </summary>
    public string? TraceId { get; set; }

    /// <summary>
    /// Gets or sets the service instance ID
    /// </summary>
    public string? ServiceInstanceId { get; set; }

    /// <summary>
    /// Gets or sets the scope name
    /// </summary>
    public string? ScopeName { get; set; }

    /// <summary>
    /// Gets or sets the span ID
    /// </summary>
    public string? SpanId { get; set; }

    /// <summary>
    /// Gets or sets the flags
    /// </summary>
    public int? Flags { get; set; }

    /// <summary>
    /// Gets or sets the telemetry SDK name
    /// </summary>
    public string? TelemetrySdkName { get; set; }

    /// <summary>
    /// Gets or sets the telemetry SDK version
    /// </summary>
    public string? TelemetrySdkVersion { get; set; }

    /// <summary>
    /// Gets or sets the telemetry SDK language
    /// </summary>
    public string? TelemetrySdkLanguage { get; set; }
}

/// <summary>
/// Extension methods for <see cref="LogsRoot"/>
/// </summary>
public static class LogsExtensions
{
    /// <summary>
    /// Gets the trace ID
    /// </summary>
    /// <param name="logsRoot">Logs root object</param>
    /// <returns>Returns the trace ID</returns>
    public static string GetTraceId(this LogsRoot logsRoot)
        => logsRoot.ResourceLogs[0].ScopeLogs[0].LogRecords[0].TraceId;

    /// <summary>
    /// Gets the resource name
    /// </summary>
    /// <param name="logsRoot">Logs root object</param>
    /// <returns>Returns the resource name</returns>
    public static string GetResourceName(this LogsRoot logsRoot)
        => logsRoot.ResourceLogs[0].Resource.GetResourceName();

    /// <summary>
    /// Gets the resource name
    /// </summary>
    /// <param name="logsRoot">Logs root object</param>
    /// <param name="timeOffset">Time offset</param>
    /// <returns>Returns the timestamp</returns>
    public static DateTime GetTimestamp(this LogsRoot logsRoot, int timeOffset)
        => DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(logsRoot.ResourceLogs[0].ScopeLogs[0].LogRecords[0].TimeUnixNano) / 1000000).AddHours(timeOffset).DateTime;

    /// <summary>
    /// Gets the resource name
    /// </summary>
    /// <param name="logsRoots">Logs root objects</param>
    /// <param name="timeOffset">Time offset</param>
    /// <param name="severity">Severity</param>
    /// <param name="filterString">Filter string</param>
    /// <param name="orderDesc">Order descending</param>
    /// <returns>Returns the structured logs</returns>
    public static List<LogView> GetStructuredLogs(
            this List<LogsRoot> logsRoots,
            int timeOffset,
            string? severity,
            string? filterString,
            bool orderDesc = true)
    {
        var logs = new List<LogView>();
        foreach (var logsRoot in logsRoots)
        {
            var resourceName = logsRoot.GetResourceName();

            foreach (var resourceLog in logsRoot.ResourceLogs)
            {
                foreach (var scopeLog in resourceLog.ScopeLogs)
                {
                    foreach (var logRecord in scopeLog.LogRecords)
                    {
                        if (logs.Any(l => l.TraceId == logRecord.TraceId))
                        {
                            continue;
                        }

                        if (severity != "all" && logRecord.SeverityText != severity)
                        {
                            continue;
                        }

                        var contains = logRecord.Body.StringValue.Contains("[REQUEST]");

                        if (logRecord.Body.StringValue.Contains("[REQUEST]")
                            || logRecord.Body.StringValue.Contains("[RESPONSE]")
                            || logRecord.Body.StringValue.Contains("[ERROR]")
                            || (filterString != null && !logRecord.Body.StringValue.ToLower().Contains(filterString.ToLower())))
                        {
                            continue;
                        }

                        DateTime dateTime = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(logRecord.TimeUnixNano) / 1000000).AddHours(timeOffset).DateTime;

                        logs.Add(new LogView
                        {
                            ResourceName = resourceName,
                            Severity = logRecord.SeverityText,
                            Timestamp = dateTime,
                            Message = logRecord.Body.StringValue,
                            TraceId = logRecord.TraceId
                        });
                    }
                }
            }
        }

        return orderDesc 
            ? logs.OrderByDescending(x => x.Timestamp).ToList() 
            : logs.OrderBy(x => x.Timestamp).ToList();
    }

    public static List<LogView> GetStructuredLogs(this List<LogsRoot> logsRoots, int timeOffset)
    {
        var logs = new List<LogView>();
        foreach (var logsRoot in logsRoots)
        {
            var resourceName = logsRoot.GetResourceName();
            foreach (var resourceLog in logsRoot.ResourceLogs)
            {
                foreach (var scopeLog in resourceLog.ScopeLogs)
                {
                    foreach (var logRecord in scopeLog.LogRecords)
                    {
                        if (logs.Any(l => l.TraceId == logRecord.TraceId))
                        {
                            continue;
                        }
                        var contains = logRecord.Body.StringValue.Contains("[REQUEST]");
                        if (logRecord.Body.StringValue.Contains("[REQUEST]")
                            || logRecord.Body.StringValue.Contains("[RESPONSE]")
                            || logRecord.Body.StringValue.Contains("[ERROR]"))
                        {
                            continue;
                        }
                        DateTime dateTime = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(logRecord.TimeUnixNano) / 1000000).AddHours(timeOffset).DateTime;
                        logs.Add(new LogView
                        {
                            ResourceName = resourceName,
                            Severity = logRecord.SeverityText,
                            Timestamp = dateTime,
                            Message = logRecord.Body.StringValue,
                            TraceId = logRecord.TraceId
                        });
                    }
                }
            }
        }
        return logs.OrderByDescending(x => x.Timestamp).ToList();
    }

    /// <summary>
    /// Gets the resources
    /// </summary>
    /// <param name="logsRoots">Logs root objects</param>
    /// <returns>Returns the resources</returns>
    public static List<string> GetResources(this List<Log> logs)
        => logs.Select(x => x.ResourceName).Distinct().ToList();

    /// <summary>
    /// Gets the request response object
    /// </summary>
    /// <param name="logsRoot">Logs root object</param>
    /// <returns>Returns the request response object</returns>
    public static RequestResponse? GetRequestResponse(this List<Log> logs)
    {
        var request = logs.Find(x => x.Message.Contains("[REQUEST]"))?.Message;
        var response = logs.Find(x => x.Message.Contains("[RESPONSE]"))?.Message;

        if (request != null || response != null)
        {
            var requestJson = request is null 
                ? null 
                : JsonObject.Parse(request.Replace("[REQUEST]", ""));
            return new RequestResponse
            {
                Request = JsonSerializer.Deserialize<Request>(requestJson) ?? null,
                Response = response?.Replace("[RESPONSE]", "") ?? null
            };
        }

        return null;
    }

    /// <summary>
    /// Gets the log details
    /// </summary>
    /// <param name="resource">Resource object</param>
    /// <param name="key">Key</param>
    /// <returns>Returns the attribute value</returns>
    public static string GetAttributeValue(this Resource resource, string key)
    {
        return resource.Attributes.Find(x => x.Key.Equals(key)).Value.StringValue;
    }

    /// <summary>
    /// Gets the log details
    /// </summary>
    /// <param name="logsRoot">Logs root object</param>
    /// <param name="logView">Log view object</param>
    /// <returns>Returns the log details view</returns>
    public static LogDetailsView GetLogDetails(this List<LogsRoot> logsRoot, LogView logView)
    {
        var logRecord = logsRoot[0].ResourceLogs[0].ScopeLogs[0].LogRecords.Find(x => x.TraceId == logView.TraceId);
        return new LogDetailsView
        {
            ResourceName = logView.ResourceName ?? null,
            Severity = logView.Severity ?? null,
            Timestamp = logView.Timestamp ?? null,
            Message = logView.Message ?? null,
            TraceId = logView.TraceId ?? null,
            ServiceInstanceId = logsRoot[0].ResourceLogs[0].Resource.GetAttributeValue("service.instance.id") ?? null,
            ScopeName = logsRoot[0].ResourceLogs[0].ScopeLogs[0].Scope.Name ?? null,
            SpanId = logRecord?.SpanId ?? null,
            Flags = logRecord?.Flags,
            TelemetrySdkName = logsRoot[0].ResourceLogs[0].Resource.GetAttributeValue("telemetry.sdk.name") ?? null,
            TelemetrySdkVersion = logsRoot[0].ResourceLogs[0].Resource.GetAttributeValue("telemetry.sdk.version") ?? null,
            TelemetrySdkLanguage = logsRoot[0].ResourceLogs[0].Resource.GetAttributeValue("telemetry.sdk.language") ?? null
        };
    }

    /// <summary>
    /// Gets the log records
    /// </summary>
    /// <param name="logsRoot">Logs root object</param>
    /// <returns>Returns the log records</returns>
    public static List<LogRecord> GetLogRecords(this LogsRoot logsRoot)
        => logsRoot.ResourceLogs[0].ScopeLogs[0].LogRecords;

    public static Log GetLog(this LogsRoot logsRoot, LogRecord logRecord)
    {
        return new Log
        {
            ResourceName = logsRoot.GetResourceName(),
            Severity = logRecord.SeverityText,
            Message = logRecord.Body.StringValue,
            CreatedAt = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(logRecord.TimeUnixNano) / 1000000).DateTime,
            Scope = logsRoot.ResourceLogs[0].ScopeLogs[0].Scope.Name,
            ServiceInstance = logsRoot.ResourceLogs[0].Resource.GetAttributeValue("service.instance.id"),
            TraceId = logRecord.TraceId,
            SpanId = logRecord.SpanId,
            Flags = logRecord.Flags,
            TelemetrySdkName = logsRoot.ResourceLogs[0].Resource.GetAttributeValue("telemetry.sdk.name"),
            TelemetrySdkLanguage = logsRoot.ResourceLogs[0].Resource.GetAttributeValue("telemetry.sdk.language"),
            TelemetrySdkVersion = logsRoot.ResourceLogs[0].Resource.GetAttributeValue("telemetry.sdk.version")
        };
    }

    public static bool ContainsReqRes(this Log log)
    {
        return log.Message.Contains("[REQUEST]")
            || log.Message.Contains("[RESPONSE]")
            || log.Message.Contains("[ERROR]");
    }
}
