﻿using ContainerControlPanel.Domain.Models;

namespace ContainerControlPanel.API.Interfaces;

/// <summary>
/// Interface for managing data store service
/// </summary>
public interface IDataStoreService
{
    /// <summary>
    /// Gets a list of saved requests
    /// </summary>
    /// <returns>Returns a list of saved requests</returns>
    Task<List<SavedRequest>> GetRequestsAsync();

    /// <summary>
    /// Saves a request
    /// </summary>
    /// <param name="request">Request to save</param>
    Task SaveRequestAsync(SavedRequest request);

    /// <summary>
    /// Deletes a request
    /// </summary>
    /// <param name="id">Request ID</param>
    Task DeleteRequestAsync(string id);

    /// <summary>
    /// Pins a request
    /// </summary>
    /// <param name="id">Request ID</param>
    Task PinRequestAsync(string id);

    /// <summary>
    /// Saves metrics
    /// </summary>
    /// <param name="metrics">Metrics to save</param>
    /// <param name="serviceName">Name of the service</param>
    /// <param name="routeName">Name of the route</param>
    Task SaveMetricsAsync(Metrics metrics, string? serviceName = "", string? routeName = "");

    /// <summary>
    /// Saves traces
    /// </summary>
    /// <param name="trace">Traces to save</param>
    /// <param name="traceId">Trace ID</param>
    Task SaveTraceAsync(Trace trace, string? traceId = "");

    /// <summary>
    /// Saves logs
    /// </summary>
    /// <param name="log">Logs to save</param>
    /// <param name="traceId">Trace ID</param>
    Task SaveLogAsync(Log log, string? traceId = "");

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
    Task<List<Trace>> GetTracesAsync(
        int timeOffset,
        string? resource,
        string? timestamp,
        bool routesOnly,
        int page,
        int pageSize);

    /// <summary>
    /// Gets a trace
    /// </summary>
    /// <param name="traceId">Trace ID</param>
    /// <returns>Returns a trace</returns>
    Task<Trace> GetTraceAsync(string traceId);

    /// <summary>
    /// Gets a list of metrics
    /// </summary>
    /// <returns>Returns a list of metrics</returns>
    Task<List<Metrics>> GetMetricsAsync();

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
    Task<List<Log>> GetLogsAsync(
        int timeOffset, 
        string? timestamp, 
        string? resource, 
        string? severity, 
        string? filter,
        int page, 
        int pageSize);

    /// <summary>
    /// Gets a list of logs by trace id
    /// </summary>
    /// <param name="traceId">Trace ID</param>
    /// <returns>Returns a list of logs</returns>
    Task<List<Log>> GetLogsByTraceAsync(string traceId);

    /// <summary>
    /// Gets a magic input
    /// </summary>
    /// <returns>Returns a magic input</returns>
    Task<string> GetMagicInput();

    /// <summary>
    /// Saves a magic input
    /// </summary>
    /// <param name="magicInput">The magic input to save</param>
    Task SaveMagicInput(string magicInput);
}
