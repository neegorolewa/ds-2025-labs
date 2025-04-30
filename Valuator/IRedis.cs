namespace Valuator.Service;

public interface IRedis
{
    string Get(string key, string? region = null);
    void Set(string key, string value, string? region = null);
    List<string> GetKeys(string pattern, string? region = null);

    string GetShardRegion(string textId);
    void SetShardMap(string textId, string region);
}
