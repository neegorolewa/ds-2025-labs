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
    private readonly IRedisService _redisService;

    public SummaryModel(ILogger<SummaryModel> logger, IRedisService redisService)
    {
        _logger = logger;
        _redisService = redisService;
    }

    public double Rank { get; set; }
    public double Similarity { get; set; }

    public void OnGet(string id)
    {
        _logger.LogDebug(id);

        // TODO: (pa1) проинициализировать свойства Rank и Similarity значениями из БД (Redis)
        string rankKey = "RANK-" + id;
        string rankValue = _redisService.Get(rankKey);
        if (double.TryParse(rankValue, out double rank))
        {
            Rank = rank;
        }
        else
        {
            Rank = 0;
        }

        string similarityKey = "SIMILARITY-" + id;
        string similarityValue = _redisService.Get(similarityKey);
        if (double.TryParse(similarityValue, out double similarity))
        {
            Similarity = similarity;
        }
        else
        {
            Similarity = 0;
        }
    }
}
