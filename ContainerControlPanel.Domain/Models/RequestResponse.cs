using System.Text.Json.Serialization;

namespace ContainerControlPanel.Domain.Models;

/// <summary>
/// Class to represent a request and response pair
/// </summary>
public class RequestResponse
{
    /// <summary>
    /// Gets or sets the request object
    /// </summary>
    public Request? Request { get; set; }

    /// <summary>
    /// Gets or sets the response string
    /// </summary>
    public string? Response { get; set; }
}

/// <summary>
/// Class to represent a request object
/// </summary>
public class Request
{
    /// <summary>
    /// Gets or sets the headers of the request
    /// </summary>
    public Headers Headers { get; set; }

    /// <summary>
    /// Gets or sets the query parameters of the request
    /// </summary>
    public List<Query> Query { get; set; }

    /// <summary>
    /// Gets or sets the stringified body of the request
    /// </summary>
    public object Body { get; set; }
}

/// <summary>
/// Class to represent the headers of a request
/// </summary>
public class Headers
{
    /// <summary>
    /// Gets or sets the Accept header
    /// </summary>
    public List<string> Accept { get; set; }

    /// <summary>
    /// Gets or sets the Connection header
    /// </summary>
    public List<string> Connection { get; set; }

    /// <summary>
    /// Gets or sets the Host header
    /// </summary>
    public List<string> Host { get; set; }

    /// <summary>
    /// Gets or sets the User-Agent header
    /// </summary>
    [JsonPropertyName("User-Agent")]
    public List<string> UserAgent { get; set; }

    /// <summary>
    /// Gets or sets the Accept-Encoding header
    /// </summary>

    [JsonPropertyName("Accept-Encoding")]
    public List<string> AcceptEncoding { get; set; }

    /// <summary>
    /// Gets or sets the Accept-Language header
    /// </summary>
    [JsonPropertyName("Accept-Language")]
    public List<string> AcceptLanguage { get; set; }

    /// <summary>
    /// Gets or sets the Content-Type header
    /// </summary>
    [JsonPropertyName("Content-Type")]
    public List<string> ContentType { get; set; }

    /// <summary>
    /// Gets or sets the Cookie header
    /// </summary>
    public List<string> Cookie { get; set; }

    /// <summary>
    /// Gets or sets the Origin header
    /// </summary>
    public List<string> Origin { get; set; }

    /// <summary>
    /// Gets or sets the Referer header
    /// </summary>
    public List<string> Referer { get; set; }

    /// <summary>
    /// Gets or sets the Content-Length header
    /// </summary>
    [JsonPropertyName("Content-Length")]
    public List<string> ContentLength { get; set; }

    /// <summary>
    /// Gets or sets the sec-ch-ua-platform header
    /// </summary>
    [JsonPropertyName("sec-ch-ua-platform")]
    public List<string> secchuaplatform { get; set; }

    /// <summary>
    /// Gets or sets the sec-ch-ua header
    /// </summary>
    [JsonPropertyName("sec-ch-ua")]
    public List<string> secchua { get; set; }

    /// <summary>
    /// Gets or sets the sec-ch-ua-mobile header
    /// </summary>
    [JsonPropertyName("sec-ch-ua-mobile")]
    public List<string> secchuamobile { get; set; }

    /// <summary>
    /// Gets or sets the Sec-Fetch-Site header
    /// </summary>
    [JsonPropertyName("Sec-Fetch-Site")]
    public List<string> SecFetchSite { get; set; }

    /// <summary>
    /// Gets or sets the Sec-Fetch-Mode header
    /// </summary>
    [JsonPropertyName("Sec-Fetch-Mode")]
    public List<string> SecFetchMode { get; set; }

    /// <summary>
    /// Gets or sets the Sec-Fetch-Dest header
    /// </summary>
    [JsonPropertyName("Sec-Fetch-Dest")]
    public List<string> SecFetchDest { get; set; }
}

/// <summary>
/// Class to represent a query parameter
/// </summary>
public class Query
{
    /// <summary>
    /// Gets or sets the key of the query parameter
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// Gets or sets the value of the query parameter
    /// </summary>
    public List<string> Value { get; set; }
}

/// <summary>
/// Extension methods for the <see cref="Headers"/> class
/// </summary>
public static class RequestResponseExtensions
{
    /// <summary>
    /// Gets the value of a header by key
    /// </summary>
    /// <param name="headers">Headers object</param>
    /// <param name="key">Key of the header</param>
    /// <returns>Returns the value of the header</returns>
    public static string GetHeaderValue(this Headers headers, string key)
        => headers.GetType().GetProperty(key)?.GetValue(headers) as string;

    /// <summary>
    /// Gets all headers as a collection of key-value pairs
    /// </summary>
    /// <param name="headers">Headers object</param>
    /// <returns>Returns a collection of key-value pairs</returns>
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
