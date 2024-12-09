using StackExchange.Redis;

public class RedisService
{
    private readonly IConnectionMultiplexer _redis;

    public RedisService(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task SetValueAsync(string key, string value, TimeSpan? expiry = null)
    {
        var db = _redis.GetDatabase();
        await db.StringSetAsync(key, value, expiry);
    }

    public async Task<string?> GetValueAsync(string key)
    {
        var db = _redis.GetDatabase();
        return await db.StringGetAsync(key);
    }

    public async Task AddToListAsync(string key, string value, TimeSpan? expiry = null)
    {
        var db = _redis.GetDatabase();
        await db.ListRightPushAsync(key, value);

        if (expiry.HasValue)
        {
            await db.KeyExpireAsync(key, expiry);
        }
    }

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
}
