﻿using ContainerControlPanel.API.Interfaces;
using ContainerControlPanel.Domain.Models;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Routing;
using StackExchange.Redis;
using System.Text.Json;

/// <summary>
/// Class for managing Redis cache
/// </summary>
public class RedisService : IDataStoreService
{
    /// <summary>
    /// Redis connection multiplexer
    /// </summary>
    private readonly IConnectionMultiplexer _redis;

    /// <summary>
    /// Initializes a new instance of the <see cref="RedisService"/> class.
    /// </summary>
    /// <param name="redis">Redis connection multiplexer</param>
    public RedisService(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    /// <summary>
    /// Saves a request
    /// </summary>
    /// <param name="request"></param>
    public async Task SaveRequestAsync(SavedRequest request)
    {
        await SetValueAsync($"request{request.Id}", JsonSerializer.Serialize(request));
    }

    /// <summary>
    /// Gets a list of saved requests
    /// </summary>
    /// <returns>Returns a list of saved requests</returns>
    public async Task<List<SavedRequest>> GetRequestsAsync()
    {
        var keys = await ScanKeysByPatternAsync("request");
        var requests = new List<SavedRequest>();

        foreach (var key in keys)
        {
            var request = JsonSerializer.Deserialize<SavedRequest>(key);
            requests.Add(request);
        }

        return requests;
    }

    /// <summary>
    /// Deletes a request
    /// </summary>
    /// <param name="id">ID of the request</param>
    public async Task DeleteRequestAsync(string id)
    {
        await RemoveKeyAsync($"request{id}");
    }

    /// <summary>
    /// Pins a request
    /// </summary>
    /// <param name="id">ID of the request</param>
    public async Task PinRequestAsync(string id)
    {
        var request = await GetValueAsync($"request{id}");
        var savedRequest = JsonSerializer.Deserialize<SavedRequest>(request);
        savedRequest.IsPinned = !savedRequest.IsPinned;
        await SetValueAsync($"request{id}", JsonSerializer.Serialize(savedRequest));
    }

    /// <summary>
    /// Saves metrics
    /// </summary>
    /// <param name="metrics">Metrics to save</param>
    /// <param name="serviceName">Name of the service</param>
    /// <param name="routeName">Name of the route</param>
    public async Task SaveMetricsAsync(Metrics metrics, string serviceName, string routeName)
    {
        string value = JsonSerializer.Serialize(metrics);
        await SetValueAsync($"metrics{serviceName}{routeName}", value, TimeSpan.FromDays(14));
        await SetValueAsync("metrics", JsonSerializer.Serialize(metrics));
    }

    /// <summary>
    /// Saves traces
    /// </summary>
    /// <param name="trace">Traces to save</param>
    /// <param name="traceId">ID of the trace</param>
    public async Task SaveTraceAsync(Trace trace, string traceId)
    {
        string value = JsonSerializer.Serialize(trace);
        await SetValueAsync($"trace{traceId}{Guid.NewGuid().ToString()}", value, TimeSpan.FromDays(14));
    }

    /// <summary>
    /// Saves logs
    /// </summary>
    /// <param name="log">Logs to save</param>
    /// <param name="traceId">Trace ID</param>
    public async Task SaveLogAsync(Log log, string traceId)
    {
        string value = JsonSerializer.Serialize(log);
        await SetValueAsync($"log{traceId}", value, TimeSpan.FromDays(14));
    }

    /// <summary>
    /// Gets traces from the MongoDB data store
    /// </summary>
    /// <param name="timeOffset">Time offset</param>
    /// <param name="resource">Name of the resource</param>
    /// <param name="timestamp">Timestamp</param>
    /// <param name="routesOnly">Flag to get only routes</param>
    /// <param name="page">Number of the page</param>
    /// <param name="pageSize">Size of the page</param>
    /// <returns>Returns a list of traces</returns>
    public async Task<List<Trace>> GetTracesAsync(
        int timeOffset,
        string? resource,
        string? timestamp,
        bool routesOnly,
        int page,
        int pageSize)
    {      
        List<Trace> traces = new();
        var result = await ScanKeysByPatternAsync("trace");

        foreach (var item in result)
        {
            var deserialized = JsonSerializer.Deserialize<Trace>(item);

            if ((resource == "all" || deserialized.ResourceName == resource)
                && (string.IsNullOrEmpty(timestamp) || deserialized.Timestamp.Date == DateTime.Parse(timestamp).Date))
                traces.Add(deserialized);
        }

        return traces;
    }

    /// <summary>
    /// Gets a trace
    /// </summary>
    /// <param name="traceId">Trace ID</param>
    /// <returns>Returns a trace</returns>
    public async Task<Trace> GetTraceAsync(string traceId)
    {
        var result = await ScanKeysByPatternAsync($"trace{traceId}");

        return JsonSerializer.Deserialize<Trace>(result.FirstOrDefault());
    }

    /// <summary>
    /// Gets a list of metrics
    /// </summary>
    /// <returns>Returns a list of metrics</returns>
    public async Task<List<Metrics>> GetMetricsAsync()
    {
        List<Metrics> metrics = new();
        var result = await ScanKeysByPatternAsync("metrics");

        foreach (var item in result)
        {
            var deserialized = JsonSerializer.Deserialize<Metrics>(item);
            metrics.Add(deserialized);
        }

        return metrics;
    }

    /// <summary>
    /// Gets a list of logs
    /// </summary>
    /// <param name="timeOffset">Time offset</param>
    /// <param name="timestamp">Timestamp</param>
    /// <param name="resource">Name of the resource</param>
    /// <param name="severity">Name of the severity</param>
    /// <param name="filter">String to filter</param>
    /// <param name="page">Number of the page</param>
    /// <param name="pageSize">Size of the page</param>
    /// <returns>Returns a list of logs</returns>
    public async Task<List<Log>> GetLogsAsync(
        int timeOffset, 
        string? timestamp, 
        string? resource, 
        string? severity,
        string? filter,
        int page, 
        int pageSize)
    {
        List<Log> logs = new();
        var result = await ScanKeysByPatternAsync("log");

        foreach (var item in result)
        {
            var deserialized = JsonSerializer.Deserialize<Log>(item);

            if ((resource == "all" || resource == deserialized.ResourceName)
                && (timestamp == "null" || deserialized.CreatedAt.AddHours(timeOffset).Date == DateTime.Parse(timestamp).Date))
                logs.Add(deserialized);
        }

        return logs;
    }

    /// <summary>
    /// Gets a log
    /// </summary>
    /// <param name="traceId">Trace ID</param>
    /// <returns>Returns a log</returns>
    public async Task<List<Log>> GetLogsByTraceAsync(string traceId)
    {
        List<Log> logs = new();
        var result = await ScanKeysByPatternAsync($"log{traceId}");

        foreach (var item in result)
        {
            var deserialized = JsonSerializer.Deserialize<Log>(item);
            logs.Add(deserialized);
        }

        return logs;
    }

    /// <summary>
    /// Gets a magic input
    /// </summary>
    /// <returns>Returns a magic input</returns>
    public async Task<string> GetMagicInput()
    {
        var result = await ScanKeysByPatternAsync("magicInput");
        return result.FirstOrDefault();
    }

    /// <summary>
    /// Saves a magic input
    /// </summary>
    /// <param name="magicInput">Magic input to save</param>
    public async Task SaveMagicInput(string magicInput)
    {
        await SetValueAsync("magicInput", magicInput);
    }

    /// <summary>
    /// Sets a value in the cache
    /// </summary>
    /// <param name="key">Key</param>
    /// <param name="value">Value</param>
    /// <param name="expiry">Expiration time</param>
    /// <returns>Returns a task</returns>
    public async Task SetValueAsync(string key, string value, TimeSpan? expiry = null)
    {
        var db = _redis.GetDatabase();
        await db.StringSetAsync(key, value, expiry);
    }

    /// <summary>
    /// Gets a value from the cache
    /// </summary>
    /// <param name="key">Key</param>
    /// <returns>Returns the value</returns>
    public async Task<string?> GetValueAsync(string key)
    {
        var db = _redis.GetDatabase();
        return await db.StringGetAsync(key);
    }

    /// <summary>
    /// Removes a value from the cache
    /// </summary>
    /// <param name="key">Key</param>
    /// <param name="value">Value</param>
    /// <param name="expiry">Expiration time</param>
    /// <returns>Returns a task</returns>
    public async Task AddToListAsync(string key, string value, TimeSpan? expiry = null)
    {
        var db = _redis.GetDatabase();
        await db.ListRightPushAsync(key, value);

        if (expiry.HasValue)
        {
            await db.KeyExpireAsync(key, expiry);
        }
    }

    /// <summary>
    /// Gets a list from the cache
    /// </summary>
    /// <param name="pattern">The pattern to search for</param>
    /// <returns>Returns a list of values</returns>
    public async Task<List<string>> ScanKeysByPatternAsync(string pattern)
    {
        var server = _redis.GetServer(_redis.GetEndPoints().First());
        var values = new List<string>();
        var db = _redis.GetDatabase();

        foreach (var key in server.Keys(pattern: $"*{pattern}*"))
        {          
            values.Add(await db.StringGetAsync(key));
        }

        return values;
    }

    /// <summary>
    /// Removes a key from the cache
    /// </summary>
    /// <param name="key">Redis key</param>
    /// <returns>Returns a task</returns>
    public async Task RemoveKeyAsync(string key)
    {
        var db = _redis.GetDatabase();
        await db.KeyDeleteAsync(key);
    }
}
