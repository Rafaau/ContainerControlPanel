using StackExchange.Redis;

/// <summary>
/// Class for managing Redis cache
/// </summary>
public class RedisService
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
