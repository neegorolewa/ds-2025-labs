namespace Valuator.Service;

public interface IRedisService
{
    string Get(string key);
    void Set(string key, string value);
    List<string> GetKeys();
}
