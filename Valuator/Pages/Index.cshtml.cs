using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StackExchange.Redis;
using Valuator.Service;


namespace Valuator.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IRedisService _rediseService;

    public IndexModel(ILogger<IndexModel> logger, IRedisService redisService)
    {
        _logger = logger;
        _rediseService = redisService;
    }

    public void OnGet()
    {

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
        double nonAlphabeticCount = text.Count(c => IsCirillicOrLatinic(c));
        double countSymbols = text.Length; 

        return countSymbols == 0 ? 0 : nonAlphabeticCount / countSymbols;
    }
        
    public string CalculateSimilarity(string text)
    {
        List<string> keys = _rediseService.GetKeys();
        foreach (var key in keys)
        {
            if (key.StartsWith("TEXT-"))
            {
                string storedValue = _rediseService.Get(key);
                if (storedValue == text)
                {
                    return "1";
                }
            }
        }
        return "0";
    }

    public bool IsCirillicOrLatinic(char c)
    {
        if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'))
        {
            return true;
        }

        if ((c >= 'А' && c <= 'Я') || (c >= 'а' && c <= 'я') || c == 'Ё' || c == 'ё')
        {
            return true;
        }

        return false;
    }
}
