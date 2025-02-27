
using StackExchange.Redis;

namespace Valuator.Service;

public class Redis : IRedis
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;

    public Redis(string redisConnectionString)
    {
        _connectionMultiplexer = ConnectionMultiplexer.Connect(redisConnectionString);
    }

    public string Get(string key)
    {
        var _db = _connectionMultiplexer.GetDatabase();
        var value = _db.StringGet(key);

        if (value.IsNullOrEmpty)
        {
            return string.Empty;
        }

        return value.ToString();
    }

    public List<string> GetKeys(string key)
    {
        var server = _connectionMultiplexer.GetServer(_connectionMultiplexer.GetEndPoints().First());
        return server.Keys(pattern: key + "*").Select(k => k.ToString()).ToList();
    }

    public void Set(string key, string value)
    {
        var db = _connectionMultiplexer.GetDatabase();
        db.StringSet(key, value);
    }
}
