using System.Globalization;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Valuator.Service;

namespace RankCalculator;

public class RankCalculator
{
    private static IRedis _redis;
    private const string QueueName = "valuator.processing.rank";

    public static async Task Main(string[] args)
    {
        var redisConfig = new Dictionary<string, string>
        {
            ["RU"] = Environment.GetEnvironmentVariable("DB_RU") ,
            ["EU"] = Environment.GetEnvironmentVariable("DB_EU") ,
            ["ASIA"] = Environment.GetEnvironmentVariable("DB_ASIA")
        };

        _redis = new Redis(
            Environment.GetEnvironmentVariable("DB_MAIN") ,
            redisConfig
        );

        Console.WriteLine("RankCalculator started");

        ConnectionFactory factory = new ConnectionFactory
        {
            HostName = "localhost",
        };

        await using IConnection connection = await factory.CreateConnectionAsync();
        await using IChannel channel = await connection.CreateChannelAsync();

        await DeclareTopologyAsync(channel);
        string consumerTag = await RunConsumer(channel);

        Console.WriteLine("Press Enter to exit");
        Console.ReadLine();

        await channel.BasicCancelAsync(consumerTag);

        Console.WriteLine("done");
    }

    private static async Task<string> RunConsumer(IChannel channel)
    {
        AsyncEventingBasicConsumer consumer = new(channel);
        consumer.ReceivedAsync += (_, eventArgs) => ConsumeAsync(channel, eventArgs);
        return await channel.BasicConsumeAsync(
            queue: QueueName,
            autoAck: false,
            consumer: consumer
        );
    }

    private static async Task ConsumeAsync(IChannel channel, BasicDeliverEventArgs eventArgs)
    {
        Console.WriteLine("Consuming");
        var id = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
        Console.WriteLine($"Processing ID: {id} ");

        string region = _redis.GetShardRegion(id);
        Console.WriteLine($"LOOKUP: {id}, {region}");

        string text = _redis.Get("TEXT-" + id, region);

        double rank = CalculateRank(text);
        _redis.Set("RANK-" + id, rank.ToString(), region);
        //
        Console.WriteLine($"Saved RANK-{id} = {rank} in region {region}");
        
        //
        string savedRank = _redis.Get($"RANK-{id}", region);
        Console.WriteLine($"Verify RANK: {savedRank}");

        await PublishRankCalculatedEvent(channel, id, rank);
        await channel.BasicAckAsync(eventArgs.DeliveryTag, false);
        
        Console.WriteLine($"Processing ID: {id} Saved text: {text} Rank: {rank}");
    }

    private static async Task PublishRankCalculatedEvent(IChannel channel, string id, double rank)
    {
        var message = $"RANK-{id}: {rank}";
        var body = Encoding.UTF8.GetBytes(message);
        await channel.BasicPublishAsync(
            exchange: "logs", 
            routingKey: string.Empty, 
            body: body
            );
        Console.WriteLine($" [x] Sent {message}");
    }

    public static double CalculateRank(string text)
    {
        if (string.IsNullOrEmpty(text))
            return 0;

        var textInfo = new StringInfo(text);
        int textLength = textInfo.LengthInTextElements;

        int nonAlphabeticCount = 0;

        for (int i = 0; i < textLength; i++)
        {
            string element = textInfo.SubstringByTextElements(i, 1);
            if (element.Length == 1)
            {
                if (!char.IsLetter(element[0]))
                    nonAlphabeticCount++;
            }
            else
            {
                nonAlphabeticCount++;
            }
        }

        return (double)nonAlphabeticCount / textLength;
    }


    /// <summary>
    ///  Определяет топологию: queue -> consumer.
    /// </summary>
    private static async Task DeclareTopologyAsync(IChannel channel)
    {
        await channel.ExchangeDeclareAsync(
            exchange: "logs",
            type: ExchangeType.Fanout
        );

        await channel.QueueDeclareAsync(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false
        );
    }
}
