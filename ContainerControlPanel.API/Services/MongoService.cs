using ContainerControlPanel.API.Interfaces;
using ContainerControlPanel.Domain.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ContainerControlPanel.API.Services;

/// <summary>
/// Class for managing MongoDB data store
/// </summary>
public class MongoService : IDataStoreService
{
    /// <summary>
    /// MongoDB database interface
    /// </summary>
    private readonly IMongoDatabase _database;

    /// <summary>
    /// Initializes a new instance of the <see cref="MongoService"/> class.
    /// </summary>
    /// <param name="configuration">Configuration</param>
    public MongoService(IConfiguration configuration)
    {
        var client = new MongoClient(configuration["MongoDB:ConnectionString"]);
        _database = client.GetDatabase("CCP_DB");
    }

    /// <summary>
    /// Initializes the MongoDB data store
    /// </summary>
    /// <returns></returns>
    public async Task InitializeAsync()
    {
        var metricsCollection = _database.GetCollection<MetricsRoot>("Metrics");
        var tracesCollection = _database.GetCollection<TracesRoot>("Traces");
        var logsCollection = _database.GetCollection<Log>("Logs");

        var metricsIndexes = await metricsCollection.Indexes.List().ToListAsync();
        bool metricsTTLIndexExists = metricsIndexes.Any(x => x["name"].AsString == "CreatedAtTTL");

        if (!metricsTTLIndexExists)
        {
            var metricsIndexModel = new CreateIndexModel<MetricsRoot>(
                Builders<MetricsRoot>.IndexKeys.Ascending(x => x.CreatedAt),
                new CreateIndexOptions { ExpireAfter = TimeSpan.FromDays(14), Name = "CreatedAtTTL" });
            await metricsCollection.Indexes.CreateOneAsync(metricsIndexModel);
        }

        var tracesIndexes = await tracesCollection.Indexes.List().ToListAsync();
        bool tracesTTLIndexExists = tracesIndexes.Any(x => x["name"].AsString == "CreatedAtTTL");

        if (!tracesTTLIndexExists)
        {
            var tracesIndexModel = new CreateIndexModel<TracesRoot>(
                Builders<TracesRoot>.IndexKeys.Ascending(x => x.CreatedAt),
                new CreateIndexOptions { ExpireAfter = TimeSpan.FromDays(14), Name = "CreatedAtTTL" });
            await tracesCollection.Indexes.CreateOneAsync(tracesIndexModel);
        }

        var logsIndexes = await logsCollection.Indexes.List().ToListAsync();
        bool logsTTLIndexExists = logsIndexes.Any(x => x["name"].AsString == "CreatedAtTTL");

        if (!logsTTLIndexExists)
        {
            var logsIndexModel = new CreateIndexModel<Log>(
                Builders<Log>.IndexKeys.Ascending(x => x.CreatedAt),
                new CreateIndexOptions { ExpireAfter = TimeSpan.FromDays(14), Name = "CreatedAtTTL" });
            await logsCollection.Indexes.CreateOneAsync(logsIndexModel);
        }
    }

    /// <summary>
    /// Saves a request to the MongoDB data store
    /// </summary>
    /// <param name="request">Request to save</param>
    public async Task SaveRequestAsync(SavedRequest request)
    {
        var collection = _database.GetCollection<SavedRequest>("Requests");
        await collection.InsertOneAsync(request);
    }

    /// <summary>
    /// Gets all requests from the MongoDB data store
    /// </summary>
    /// <returns>Returns a list of requests</returns>
    public async Task<List<SavedRequest>> GetRequestsAsync()
    {
        var collection = _database.GetCollection<SavedRequest>("Requests");
        return await collection.Aggregate().ToListAsync();
    }

    /// <summary>
    /// Deletes a request from the MongoDB data store
    /// </summary>
    /// <param name="id">ID of the request to delete</param>
    public async Task DeleteRequestAsync(string id)
    {
        var collection = _database.GetCollection<SavedRequest>("Requests");
        await collection.DeleteOneAsync(x => x.Id == id);
    }

    /// <summary>
    /// Pins a request in the MongoDB data store
    /// </summary>
    /// <param name="id">Identifier of the request to pin</param>
    public async Task PinRequestAsync(string id)
    {
        var collection = _database.GetCollection<SavedRequest>("Requests");
        var request = await collection.Find(x => x.Id == id).FirstOrDefaultAsync();
        request.IsPinned = !request.IsPinned;
        await collection.ReplaceOneAsync(x => x.Id == id, request);
    }

    /// <summary>
    /// Saves metrics to the MongoDB data store
    /// </summary>
    /// <param name="metrics">Metrics to save</param>
    /// <param name="serviceName">Name of the service</param>
    /// <param name="routeName">Name of the route</param>
    public async Task SaveMetricsAsync(MetricsRoot metrics, string? serviceName = "", string? routeName = "")
    {
        var collection = _database.GetCollection<MetricsRoot>("Metrics");
        await collection.InsertOneAsync(metrics);
    }

    /// <summary>
    /// Saves a trace to the MongoDB data store
    /// </summary>
    /// <param name="trace">Trace to save</param>
    /// <param name="traceId">Trace ID</param>
    public async Task SaveTraceAsync(Trace trace, string? traceId = "")
    {
        trace.Id = traceId;
        var collection = _database.GetCollection<Trace>("Traces");
        await collection.InsertOneAsync(trace);
    }

    /// <summary>
    /// Saves a log to the MongoDB data store
    /// </summary>
    /// <param name="log">Log to save</param>
    /// <param name="traceId">Trace ID</param>
    public async Task SaveLogAsync(Log log, string? traceId = "")
    {
        var collection = _database.GetCollection<Log>("Logs");
        await collection.InsertOneAsync(log);
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
        var collection = _database.GetCollection<Trace>("Traces");

        var filterBuilder = Builders<Trace>.Filter;
        var filterD = filterBuilder.Empty;

        if (!string.IsNullOrEmpty(timestamp) && timestamp != null)
        {
            DateTime targetDate = DateTime.Parse(timestamp).Date;
            DateTime nextDay = targetDate.AddDays(1);

            filterD &= filterBuilder.Gte(x => x.CreatedAt, targetDate) &
                      filterBuilder.Lt(x => x.CreatedAt, nextDay);
        }

        if (!string.IsNullOrEmpty(resource) && resource != "all")
        {
            filterD &= filterBuilder.Eq(x => x.ResourceName, resource);
        }

        if (routesOnly)
        {
            var requestWordCountFilter = filterBuilder.Regex(x => x.Request, new MongoDB.Bson.BsonRegularExpression(@"^\S+\s+\S+"));
            filterD &= requestWordCountFilter;
        }
        
        var query = collection.Find(filterD).SortByDescending(x => x.CreatedAt);

        if (page > 0 && pageSize > 0)
        {
            query.Skip((page - 1) * pageSize).Limit(pageSize);
        }

        return await query.ToListAsync();
    }

    /// <summary>
    /// Gets a trace from the MongoDB data store
    /// </summary>
    /// <param name="traceId">Trace ID</param>
    /// <returns>Returns a list of traces</returns>
    public async Task<Trace> GetTraceAsync(string traceId)
    {
        var collection = _database.GetCollection<Trace>("Traces");
        return await collection.Find(x => x.Id == traceId).FirstOrDefaultAsync();
    }

    /// <summary>
    /// Gets metrics from the MongoDB data store
    /// </summary>
    /// <returns>Returns a list of metrics</returns>
    public async Task<List<MetricsRoot>> GetMetricsAsync()
    {
        var collection = _database.GetCollection<MetricsRoot>("Metrics");
        return await collection.Aggregate().ToListAsync();
    }

    /// <summary>
    /// Gets logs from the MongoDB data store
    /// </summary>
    /// <param name="timeOffset">Time offset</param>
    /// <param name="timestamp">Timestamp</param>
    /// <param name="resource">Name of the resource</param>
    /// <param name="severity">Name of the severity</param>
    /// <param name="filter">Search filter</param>
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
        var collection = _database.GetCollection<Log>("Logs");

        var filterBuilder = Builders<Log>.Filter;
        var filterD = filterBuilder.Empty;

        if (!string.IsNullOrEmpty(timestamp) && timestamp != "null")
        {
            DateTime targetDate = DateTime.Parse(timestamp).Date;
            DateTime nextDay = targetDate.AddDays(1);

            filterD &= filterBuilder.Gte(x => x.CreatedAt, targetDate) &
                      filterBuilder.Lt(x => x.CreatedAt, nextDay);
        }

        if (!string.IsNullOrEmpty(resource) && resource != "all")
        {
            filterD &= filterBuilder.Eq(x => x.ResourceName, resource);
        }

        if (!string.IsNullOrEmpty(severity) && severity != "all")
        {
            filterD &= filterBuilder.Eq(x => x.Severity, severity);
        }

        if (!string.IsNullOrEmpty(filter))
        {
            filterD &= filterBuilder.Regex(x => x.Message, new BsonRegularExpression(filter, "i"));
        }

        var query = collection.Find(filterD).SortByDescending(x => x.CreatedAt);

        if (page > 0 && pageSize > 0)
        {
            query.Skip((page - 1) * pageSize).Limit(pageSize);
        }

        return await query.ToListAsync();
    }

    /// <summary>
    /// Gets a log from the MongoDB data store
    /// </summary>
    /// <param name="traceId">Trace ID</param>
    /// <returns>Returns a log</returns>
    public async Task<List<Log>> GetLogsByTraceAsync(string traceId)
    {
        var collection = _database.GetCollection<Log>("Logs");
        return await collection.Find(x => x.TraceId == traceId).ToListAsync();
    }
}
