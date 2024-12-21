using System.Text.Json;
using System.Text.Json.Nodes;
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

public class LogView
{
    public string? ResourceName { get; set; }
    public string? Severity { get; set; }
    public DateTime? Timestamp { get; set; }
    public string? Message { get; set; }
    public string? TraceId { get; set; }
}

public class LogDetailsView
{
    public string? ResourceName { get; set; }
    public string? Severity { get; set; }
    public DateTime? Timestamp { get; set; }
    public string? Message { get; set; }
    public string? TraceId { get; set; }
    public string? ServiceInstanceId { get; set; }
    public string? ScopeName { get; set; }
    public string? SpanId { get; set; }
    public int? Flags { get; set; }
    public string? TelemetrySdkName { get; set; }
    public string? TelemetrySdkVersion { get; set; }
    public string? TelemetrySdkLanguage { get; set; }
}

public static class LogsExtensions
{
    public static string GetTraceId(this LogsRoot logsRoot)
        => logsRoot.ResourceLogs[0].ScopeLogs[0].LogRecords[0].TraceId;

    public static string GetResourceName(this LogsRoot logsRoot)
        => logsRoot.ResourceLogs[0].Resource.GetResourceName();

    public static List<LogView> GetStructuredLogs(
            this List<LogsRoot> logsRoots,
            int timeOffset,
            string? resource,
            string? severity,
            DateTime? timestamp,
            string? filterString,
            bool orderDesc = true)
    {
        var logs = new List<LogView>();
        foreach (var logsRoot in logsRoots)
        {
            var resourceName = logsRoot.GetResourceName();

            if (resource != "all" && resourceName != resource)
            {
                continue;
            }

            foreach (var resourceLog in logsRoot.ResourceLogs)
            {
                foreach (var scopeLog in resourceLog.ScopeLogs)
                {
                    foreach (var logRecord in scopeLog.LogRecords)
                    {
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

                        if (timestamp != null && dateTime.Date != timestamp.Value.Date)
                        {
                            continue;
                        }

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

    public static List<string> GetResources(this List<LogsRoot> logsRoots)
    {
        var resources = new List<string>();
        foreach (var logsRoot in logsRoots)
        {
            if (!resources.Contains(logsRoot.GetResourceName()))
                resources.Add(logsRoot.GetResourceName());
        }
        return resources;
    }

    public static RequestResponse? GetRequestResponse(this LogsRoot logsRoot)
    {
        var request = logsRoot.ResourceLogs[0].ScopeLogs[0].LogRecords.Find(x => x.Body.StringValue.Contains("[REQUEST]"));
        var response = logsRoot.ResourceLogs[0].ScopeLogs[0].LogRecords.Find(x => x.Body.StringValue.Contains("[RESPONSE]"));

        if (request != null && response != null)
        {
            var requestJson = JsonObject.Parse(request.Body.StringValue.Replace("[REQUEST]", ""));
            return new RequestResponse
            {
                Request = JsonSerializer.Deserialize<Request>(requestJson),
                Response = response.Body.StringValue.Replace("[RESPONSE]", "")
            };
        }

        return null;
    }

    public static string GetAttributeValue(this Resource resource, string key)
    {
        return resource.Attributes.Find(x => x.Key.Equals(key)).Value.StringValue;
    }

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
}
