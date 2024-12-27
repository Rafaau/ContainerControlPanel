using System.Text.Json.Serialization;

namespace ContainerControlPanel.Domain.Models;

public class RequestResponse
{
    public Request? Request { get; set; }
    public string? Response { get; set; }
}

    public class Request
{
    public Headers Headers { get; set; }
    public List<Query> Query { get; set; }
    public object Body { get; set; }
}

public class Headers
{
    public List<string> Accept { get; set; }
    public List<string> Connection { get; set; }
    public List<string> Host { get; set; }

    [JsonPropertyName("User-Agent")]
    public List<string> UserAgent { get; set; }

    [JsonPropertyName("Accept-Encoding")]
    public List<string> AcceptEncoding { get; set; }

    [JsonPropertyName("Accept-Language")]
    public List<string> AcceptLanguage { get; set; }

    [JsonPropertyName("Content-Type")]
    public List<string> ContentType { get; set; }
    public List<string> Cookie { get; set; }
    public List<string> Origin { get; set; }
    public List<string> Referer { get; set; }

    [JsonPropertyName("Content-Length")]
    public List<string> ContentLength { get; set; }

    [JsonPropertyName("sec-ch-ua-platform")]
    public List<string> secchuaplatform { get; set; }

    [JsonPropertyName("sec-ch-ua")]
    public List<string> secchua { get; set; }

    [JsonPropertyName("sec-ch-ua-mobile")]
    public List<string> secchuamobile { get; set; }

    [JsonPropertyName("Sec-Fetch-Site")]
    public List<string> SecFetchSite { get; set; }

    [JsonPropertyName("Sec-Fetch-Mode")]
    public List<string> SecFetchMode { get; set; }

    [JsonPropertyName("Sec-Fetch-Dest")]
    public List<string> SecFetchDest { get; set; }
}

public class Query
{
    public string Key { get; set; }
    public List<string> Value { get; set; }
}

public static class RequestResponseExtensions
{
    public static string GetHeaderValue(this Headers headers, string key)
        => headers.GetType().GetProperty(key)?.GetValue(headers) as string;

    public static IEnumerable<(string Key, string Value)> GetHeaders(this Headers headers)
    {
        var properties = headers.GetType().GetProperties();

        foreach (var property in properties)
        {
            var value = property.GetValue(headers);
            if (value is List<string> list)
            {
                yield return (property.Name, list[0]);
            }
        }
    }
}
