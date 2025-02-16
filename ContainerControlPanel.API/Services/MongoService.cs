using ContainerControlPanel.API.Interfaces;
using ContainerControlPanel.Domain.Models;
using MongoDB.Driver;

namespace ContainerControlPanel.API.Services;

public class MongoService : IDataStoreService
{
    private readonly IMongoDatabase _database;

    public MongoService(IConfiguration configuration)
    {
        var client = new MongoClient(configuration["MongoDB:ConnectionString"]);
        _database = client.GetDatabase("CCP_DB");
    }

    public async Task InitializeAsync()
    {
        var metricsCollection = _database.GetCollection<MetricsRoot>("Metrics");
        var tracesCollection = _database.GetCollection<TracesRoot>("Traces");
        var logsCollection = _database.GetCollection<LogsRoot>("Logs");

        var metricsIndexes = await metricsCollection.Indexes.List().ToListAsync();
        bool metricsTTLIndexExists = metricsIndexes.Any(x => x["name"].AsString == "CreatedAtTTL");

        if (!metricsTTLIndexExists)
        {
            var metricsIndexModel = new CreateIndexModel<MetricsRoot>(
                Builders<MetricsRoot>.IndexKeys.Ascending(x => x.CreatedAt),
                new CreateIndexOptions { ExpireAfter = TimeSpan.FromDays(14) });
            await metricsCollection.Indexes.CreateOneAsync(metricsIndexModel);
        }

        var tracesIndexes = await tracesCollection.Indexes.List().ToListAsync();
        bool tracesTTLIndexExists = tracesIndexes.Any(x => x["name"].AsString == "CreatedAtTTL");

        if (!tracesTTLIndexExists)
        {
            var tracesIndexModel = new CreateIndexModel<TracesRoot>(
                Builders<TracesRoot>.IndexKeys.Ascending(x => x.CreatedAt),
                new CreateIndexOptions { ExpireAfter = TimeSpan.FromDays(14) });
            await tracesCollection.Indexes.CreateOneAsync(tracesIndexModel);
        }

        var logsIndexes = await logsCollection.Indexes.List().ToListAsync();
        bool logsTTLIndexExists = logsIndexes.Any(x => x["name"].AsString == "CreatedAtTTL");

        if (!logsTTLIndexExists)
        {
            var logsIndexModel = new CreateIndexModel<LogsRoot>(
                Builders<LogsRoot>.IndexKeys.Ascending(x => x.CreatedAt),
                new CreateIndexOptions { ExpireAfter = TimeSpan.FromDays(14) });
            await logsCollection.Indexes.CreateOneAsync(logsIndexModel);
        }
    }

    public async Task SaveRequestAsync(SavedRequest request)
    {
        var collection = _database.GetCollection<SavedRequest>("Requests");
        await collection.InsertOneAsync(request);
    }

    public async Task<List<SavedRequest>> GetRequestsAsync()
    {
        var collection = _database.GetCollection<SavedRequest>("Requests");
        return await collection.Aggregate().ToListAsync();
    }

    public async Task DeleteRequestAsync(string id)
    {
        var collection = _database.GetCollection<SavedRequest>("Requests");
        await collection.DeleteOneAsync(x => x.Id == id);
    }

    public async Task PinRequestAsync(string id)
    {
        var collection = _database.GetCollection<SavedRequest>("Requests");
        var request = await collection.Find(x => x.Id == id).FirstOrDefaultAsync();
        request.IsPinned = !request.IsPinned;
        await collection.ReplaceOneAsync(x => x.Id == id, request);
    }

    public async Task SaveMetricsAsync(MetricsRoot metrics, string? serviceName = "", string? routeName = "")
    {
        var collection = _database.GetCollection<MetricsRoot>("Metrics");
        await collection.InsertOneAsync(metrics);
    }

    public async Task SaveTraceAsync(TracesRoot trace, string? traceId = "")
    {
        trace.Id = traceId;
        var collection = _database.GetCollection<TracesRoot>("Traces");
        await collection.InsertOneAsync(trace);
    }

    public async Task SaveLogAsync(LogsRoot log, string? traceId = "")
    {
        log.Id = traceId;
        var collection = _database.GetCollection<LogsRoot>("Logs");
        await collection.InsertOneAsync(log);
    }

    public async Task<List<TracesRoot>> GetTracesAsync(int timeOffset, string? resource, string? timestamp)
    {
        var collection = _database.GetCollection<TracesRoot>("Traces");
        return await collection.Aggregate().ToListAsync();
    }

    public async Task<List<TracesRoot>> GetTraceAsync(string traceId)
    {
        var collection = _database.GetCollection<TracesRoot>("Traces");
        return await collection.Find(x => x.Id == traceId).ToListAsync();
    }

    public async Task<List<MetricsRoot>> GetMetricsAsync()
    {
        var collection = _database.GetCollection<MetricsRoot>("Metrics");
        return await collection.Aggregate().ToListAsync();
    }

    public async Task<List<LogsRoot>> GetLogsAsync(int timeOffset, string? timestamp, string? resource)
    {
        var collection = _database.GetCollection<LogsRoot>("Logs");
        return await collection.Aggregate().ToListAsync();
    }

    public async Task<LogsRoot> GetLogAsync(string traceId)
    {
        var collection = _database.GetCollection<LogsRoot>("Logs");
        return await collection.Find(x => x.Id == traceId).FirstOrDefaultAsync();
    }
}
