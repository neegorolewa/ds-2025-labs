using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

const string QueueLoggerName = "events.logger.queue";

var factory = new ConnectionFactory
{ 
    HostName = "localhost",
    UserName = Environment.GetEnvironmentVariable("RABBITMQ_DEFAULT_USER"),
    Password = Environment.GetEnvironmentVariable("RABBITMQ_DEFAULT_PASS")
};
using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

await channel.ExchangeDeclareAsync(exchange: "logs",
    type: ExchangeType.Fanout);

// declare a server-named queue
QueueDeclareOk queueDeclareResult = await channel.QueueDeclareAsync
    (
            queue: QueueLoggerName,
            durable: true,
            exclusive: false,
            autoDelete: false
    );
await channel.QueueBindAsync(queue: QueueLoggerName, exchange: "logs", routingKey: string.Empty);

Console.WriteLine(" [*] Waiting for logs.");

var consumer = new AsyncEventingBasicConsumer(channel);
consumer.ReceivedAsync += (model, ea) =>
{
    byte[] body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    Console.WriteLine($" [x] {message}");
    return Task.CompletedTask;
};

await channel.BasicConsumeAsync(QueueLoggerName, autoAck: true, consumer: consumer);

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();