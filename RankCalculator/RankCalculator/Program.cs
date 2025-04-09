
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StackExchange.Redis;

namespace RankCalculator;

class Program
{
    private static readonly IConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost:6379");
    private static readonly IDatabase db = redis.GetDatabase();
    private const string QueueName = "valuator.processing.rank";
    public static async Task Main(string[] args)
    {
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


        //получать текст здесь из бд
        string text = db.StringGet("TEXT-" + id);

        double rank = CalculateRank(text);
        db.StringSet("RANK-" + id, rank.ToString());

        await channel.BasicAckAsync(eventArgs.DeliveryTag, false);
        Console.WriteLine($"Processing ID: {id} Saved text: {text} Rank: {rank}");
    }

    public static double CalculateRank(string text)
    {
        double nonAlphabeticCount = text.Count(c => !char.IsLetter(c));
        double countSymbols = text.Length;

        return countSymbols == 0 ? 0 : nonAlphabeticCount / countSymbols;
    }


    /// <summary>
    ///  Определяет топологию: queue -> consumer.
    /// </summary>
    private static async Task DeclareTopologyAsync(IChannel channel)
    {
        await channel.QueueDeclareAsync(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false
        );
    }
    private class Data
    {
        public string Id {  get; set; }
        public string Text{ get; set; }
    }
}
