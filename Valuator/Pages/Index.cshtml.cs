using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StackExchange.Redis;
using Valuator.Service;


namespace Valuator.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly Service.IRedis _rediseService;

    public IndexModel(ILogger<IndexModel> logger, Service.IRedis redisService)
    {
        _logger = logger;
        _rediseService = redisService;
    }

    public IActionResult OnPost(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Page();
        }

        _logger.LogDebug(text);

        string id = Guid.NewGuid().ToString();

        //проверка на плагиат прежде, чем сохраняем текст в бд
        string similarityKey = "SIMILARITY-" + id;
        // TODO: (pa1) посчитать similarity и сохранить в БД (Redis) по ключу similarityKey
        string similarity = CalculateSimilarity(text);
        _rediseService.Set(similarityKey, similarity);

        string textKey = "TEXT-" + id;
        // TODO: (pa1) сохранить в БД (Redis) text по ключу textKey
        _rediseService.Set(textKey, text);

        string rankKey = "RANK-" + id;
        // TODO: (pa1) посчитать rank и сохранить в БД (Redis) по ключу rankKey
        string rank = CalculateRank(text).ToString();
        _rediseService.Set(rankKey, rank);
            

        return Redirect($"summary?id={id}");
    }

    public double CalculateRank(string text)
    {
        double nonAlphabeticCount = text.Count(c => !char.IsLetter(c));
        double countSymbols = text.Length; 

        return countSymbols == 0 ? 0 : nonAlphabeticCount / countSymbols;
    }
        
    public string CalculateSimilarity(string text)
    {
        List<string> keys = _rediseService.GetKeys("TEXT-");
        foreach (var key in keys)
        {
            string storedValue = _rediseService.Get(key);
            if (storedValue == text)
            {
                return "1";
            }
        }
        return "0";
    }
}
