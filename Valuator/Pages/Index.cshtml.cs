using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StackExchange.Redis;
using Valuator.Service;
using RabbitMQ.Client;
using System.Text.Json;
using System.Text;
using RabbitMQ.Client.Exceptions;

namespace Valuator.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly Service.IRedis _redisService;
    private const string ExchangeName = "valuator.processing.rank";
    private const string QueueName = "valuator.processing.rank";

    public IndexModel(ILogger<IndexModel> logger, Service.IRedis redisService)
    {
        _logger = logger;
        _redisService = redisService;
    }

    public async Task<IActionResult> OnPostAsync(string text, string region)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Page();
        }

        Console.WriteLine($"Received country: {region}");

        string reg = region switch
        {
            "Russia" => "RU",
            "France" or "Germany" => "EU",
            "UAE" or "India" => "ASIA",
            _ => "RU" // Дефолтный регион
        };

        Console.WriteLine($"Mapped to region: {reg}");

        _logger.LogDebug(text);
        string id = Guid.NewGuid().ToString();

        _redisService.SetShardMap(id, reg);

        //проверка на плагиат прежде, чем сохраняем текст в бд
        string similarityKey = "SIMILARITY-" + id;
        // TODO: (pa1) посчитать similarity и сохранить в БД (Redis) по ключу similarityKey
        string similarity = CalculateSimilarity(text, reg);
        _redisService.Set(similarityKey, similarity, reg);

        string textKey = "TEXT-" + id;
        // TODO: (pa1) сохранить в БД (Redis) text по ключу textKey
        _redisService.Set(textKey, text, reg);

        await SendRankCalculationTask(id, similarity);

        return Redirect($"summary?id={id}");
    }

    private static async Task PublishSimilarityCalculatedEvent(IChannel channel, string id, string similarity)
    {
        var message = $"SIMILARITY-{id}: {similarity}";
        var body = Encoding.UTF8.GetBytes(message);
        await channel.BasicPublishAsync(exchange: "logs", routingKey: string.Empty, body: body);
        Console.WriteLine($" [x] Sent {message}");
    }

    private static async Task SendRankCalculationTask(string id, string similarity)
    {
        // Установка соединения с RabbitMQ по адресу localhost:5672
        ConnectionFactory factory = new ConnectionFactory
        {
            HostName = "localhost",
            UserName = Environment.GetEnvironmentVariable("RABBITMQ_DEFAULT_USER"),
            Password = Environment.GetEnvironmentVariable("RABBITMQ_DEFAULT_PASS")
        };

        await using IConnection connection = await factory.CreateConnectionAsync();
        await using IChannel channel = await connection.CreateChannelAsync();

        await DeclareTopologyAsync(channel, CancellationToken.None);

        var body = Encoding.UTF8.GetBytes(id);

        await PublishSimilarityCalculatedEvent(channel, id, similarity);

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

    public string CalculateSimilarity(string text, string region)
    {
        List<string> keys = _redisService.GetKeys("TEXT-", region);
        foreach (var key in keys)
        {
            string storedValue = _redisService.Get(key, region);
            if (storedValue == text)
            {
                return "1";
            }
        }
        return "0";
    }
}
