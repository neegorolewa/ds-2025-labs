
using StackExchange.Redis;

namespace Valuator.Service;

public class Redis : IRedis
{
    private readonly IConnectionMultiplexer _mainConnection;
    private readonly Dictionary<string, IConnectionMultiplexer> _regionConnections;

    public Redis(string redisConnectionString, Dictionary<string, string> regionConnections)
    {
        _mainConnection = ConnectionMultiplexer.Connect(
            $"{redisConnectionString},password={GetRedisPassword("MAIN")},abortConnect=false"
        );

        _regionConnections = new Dictionary<string, IConnectionMultiplexer>();
        foreach (var region in regionConnections)
        {
            try
            {
                var connection = ConnectionMultiplexer.Connect(
                    $"{region.Value},password={GetRedisPassword(region.Key)},abortConnect=false"
                );
                _regionConnections[region.Key] = connection;
                Console.WriteLine($"Successfully connected to {region.Key} Redis");
            }
            catch (RedisConnectionException ex)
            {
                Console.WriteLine($"Failed to connect to {region.Key} Redis: {ex.Message}");
            }
        }
    }

    private string GetRedisPassword(string region)
    {
        return region.ToUpper() switch
        {
            "MAIN" => Environment.GetEnvironmentVariable("REDIS_MAIN_PASS"),
            "RU" => Environment.GetEnvironmentVariable("REDIS_RU_PASS"),
            "EU" => Environment.GetEnvironmentVariable("REDIS_EU_PASS"),
            "ASIA" => Environment.GetEnvironmentVariable("REDIS_ASIA_PASS"),
            "USERS" => Environment.GetEnvironmentVariable("REDIS_USERS_PASS"),
            _ => throw new ArgumentException($"Unknown Redis region: {region}")
        };
    }

    public string GetShardRegion(string textId)
    {
        var db = _mainConnection.GetDatabase();
        var region = db.StringGet($"shard:{textId}");
        return region.IsNullOrEmpty ? "RU" : region.ToString();
    }

    public void SetShardMap(string textId, string region)
    {
        var db = _mainConnection.GetDatabase();
        db.StringSet($"shard:{textId}", region);
        Console.WriteLine($"LOOKUP: {textId}, {region}"); // Логирование по заданию
    }

    public string Get(string key, string? region = null)
    {
        IDatabase db;
        if (region == null || !_regionConnections.ContainsKey(region))
        {
            db = _mainConnection.GetDatabase();
        }
        else
        {
            db = _regionConnections[region].GetDatabase();
        }

        var value = db.StringGet(key);
        return value.IsNullOrEmpty ? string.Empty : value.ToString();
    }

    public List<string> GetKeys(string pattern, string? region = null)
    {
        IEnumerable<RedisKey> keys;
        if (region == null || !_regionConnections.ContainsKey(region))
        {
            var server = _mainConnection.GetServer(_mainConnection.GetEndPoints().First());
            keys = server.Keys(pattern: pattern + "*");
        }
        else
        {
            var server = _regionConnections[region].GetServer(_regionConnections[region].GetEndPoints().First());
            keys = server.Keys(pattern: pattern + "*");
        }

        return keys.Select(k => k.ToString()).ToList();
    }

    public void Set(string key, string value, string? region = null)
    {
        IDatabase db;
        if (region == null || !_regionConnections.ContainsKey(region))
        {
            db = _mainConnection.GetDatabase();
        }
        else
        {
            db = _regionConnections[region].GetDatabase();
        }

        db.StringSet(key, value);
    }

    public bool SetContains(string key, string value, string? region = null)
    {
        IDatabase db;
        if (region == null || !_regionConnections.ContainsKey(region))
        {
            db = _mainConnection.GetDatabase();
        }
        else
        {
            db = _regionConnections[region].GetDatabase();
        }

        return db.SetContains(key, value);
    }
}
