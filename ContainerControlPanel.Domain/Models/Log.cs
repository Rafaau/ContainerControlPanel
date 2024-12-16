using System.Text.Json.Serialization;

namespace ContainerControlPanel.Domain.Models;

public class LogsRoot
{
    [JsonPropertyName("resourceLogs")]
    public List<ResourceLog> ResourceLogs { get; set; }
}


public class Body
{
    [JsonPropertyName("stringValue")]
    public string StringValue { get; set; }
}

public class LogRecord
{
    [JsonPropertyName("timeUnixNano")]
    public string TimeUnixNano { get; set; }

    [JsonPropertyName("severityNumber")]
    public string SeverityNumber { get; set; }

    [JsonPropertyName("severityText")]
    public string SeverityText { get; set; }

    [JsonPropertyName("body")]
    public Body Body { get; set; }

    [JsonPropertyName("flags")]
    public int Flags { get; set; }

    [JsonPropertyName("traceId")]
    public string TraceId { get; set; }

    [JsonPropertyName("spanId")]
    public string SpanId { get; set; }

    [JsonPropertyName("observedTimeUnixNano")]
    public string ObservedTimeUnixNano { get; set; }
}

public class ResourceLog
{
    [JsonPropertyName("resource")]
    public Resource Resource { get; set; }

    [JsonPropertyName("scopeLogs")]
    public List<ScopeLog> ScopeLogs { get; set; }
}

public class ScopeLog
{
    [JsonPropertyName("scope")]
    public Scope Scope { get; set; }

    [JsonPropertyName("logRecords")]
    public List<LogRecord> LogRecords { get; set; }
}

public static class LogsExtensions
{
    public static string GetTraceId(this LogsRoot logsRoot)
        => logsRoot.ResourceLogs[0].ScopeLogs[0].LogRecords[0].TraceId;

    public static string GetResourceName(this LogsRoot logsRoot)
        => logsRoot.ResourceLogs[0].Resource.GetResourceName();

    public static List<(string ResourceName,
        string Severity,
        DateTime Timestamp,
        string Message,
        string TraceId)> GetStructuredLogs(this List<LogsRoot> logsRoots)
    {
        var logs = new List<(string ResourceName, string Severity, DateTime Timestamp, string Message, string TraceId)>();
        foreach (var logsRoot in logsRoots)
        {
            var resourceName = logsRoot.GetResourceName();
            foreach (var resourceLog in logsRoot.ResourceLogs)
            {
                foreach (var scopeLog in resourceLog.ScopeLogs)
                {
                    foreach (var logRecord in scopeLog.LogRecords)
                    {
                        logs.Add((resourceName, logRecord.SeverityText,
                            DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(logRecord.TimeUnixNano) / 1000000).DateTime,
                            logRecord.Body.StringValue, logRecord.TraceId));
                    }
                }
            }
        }
        return logs;
    }
}
