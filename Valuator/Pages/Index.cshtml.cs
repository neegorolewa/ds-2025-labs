using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StackExchange.Redis;
using Valuator.Service;
using RabbitMQ.Client;
using System.Text.Json;
using System.Text;



namespace Valuator.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly Service.IRedis _rediseService;
    private const string ExchangeName = "valuator.processing.rank";
    private const string QueueName = "valuator.processing.rank";

    public IndexModel(ILogger<IndexModel> logger, Service.IRedis redisService)
    {
        _logger = logger;
        _rediseService = redisService;
    }

    public async Task<IActionResult> OnPostAsync(string text)
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

        await SendRankCalculationTask(id);

        return Redirect($"summary?id={id}");
    }

    private static async Task SendRankCalculationTask(string id)
    {
        // Установка соединения с RabbitMQ по адресу localhost:5672
        ConnectionFactory factory = new ConnectionFactory
        {
            HostName = "localhost"
        };

        await using IConnection connection = await factory.CreateConnectionAsync();
        await using IChannel channel = await connection.CreateChannelAsync();

        await DeclareTopologyAsync(channel, CancellationToken.None);

        var body = Encoding.UTF8.GetBytes(id);

        await channel.BasicPublishAsync(
                exchange: ExchangeName,
                routingKey: "",
                body: body
        );
    }

    private static async Task DeclareTopologyAsync(IChannel channel, CancellationToken ct)
    {
        await channel.ExchangeDeclareAsync(
            exchange: ExchangeName,
            type: ExchangeType.Direct,
            cancellationToken: ct
        );
        await channel.QueueDeclareAsync(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: ct
        );
        await channel.QueueBindAsync(
            queue: QueueName,
            exchange: ExchangeName,
            routingKey: "",
            cancellationToken: ct);
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
