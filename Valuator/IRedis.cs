namespace Valuator.Service;

public interface IRedis
{
    string Get(string key);
    void Set(string key, string value);
    List<string> GetKeys(string pattern);
}
