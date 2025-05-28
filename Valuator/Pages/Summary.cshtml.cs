using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Valuator.Service;

namespace Valuator.Pages;
public class SummaryModel : PageModel
{
    private readonly ILogger<SummaryModel> _logger;
    private readonly IRedis _redisService;

    public SummaryModel(ILogger<SummaryModel> logger, IRedis redisService)
    {
        _logger = logger;
        _redisService = redisService;
    }

    public double Rank { get; set; }
    public double Similarity { get; set; }
    public bool IsRankCalculated{ get; set; }

    public IActionResult OnGet(string id)
    {
        string? usernameActual = User.Identity.Name;

        if (string.IsNullOrEmpty(usernameActual))
        {
            return RedirectToPage("/Login");
        }

        _logger.LogDebug(id);

        string region = _redisService.GetShardRegion(id);

        string author = _redisService.Get($"USER-{id}", region) ?? string.Empty;

        if (author != usernameActual)
        {
            //return StatusCode(403, "Вы не автор этого текста");
            return Forbid();
        }

        // TODO: (pa1) проинициализировать свойства Rank и Similarity значениями из БД (Redis)
        Console.WriteLine($"LOOKUP: {id}, {region}");
        
        string rankKey = "RANK-" + id;
        string rankValue = _redisService.Get(rankKey, region);
        
        if (string.IsNullOrEmpty(rankValue))
        {
            IsRankCalculated = false;
        }
        else if (double.TryParse(rankValue, out double rank))
        {
            Rank = rank;
            IsRankCalculated = true;
        }
        else
        {
            IsRankCalculated = false;
        }

        string similarityKey = "SIMILARITY-" + id;
        string similarityValue = _redisService.Get(similarityKey, region);
        if (double.TryParse(similarityValue, out double similarity))
        {
            Similarity = similarity;
        }
        else
        {
            Similarity = 0;
        }

        return Page();
    }
}
