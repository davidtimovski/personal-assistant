using System.Text;
using System.Text.Json;
using Core.Application.Contracts;
using Core.Application.Contracts.Models.Sender;
using Core.Infrastructure.Configuration;
using RabbitMQ.Client;

namespace Core.Infrastructure.Sender;

public class SenderService : ISenderService
{
    private readonly SenderConfiguration _config;

    public SenderService(SenderConfiguration config)
    {
        _config = config;
    }

    public async Task EnqueueAsync(ISendable sendable)
    {
        switch (sendable)
        {
            case Email email:
                await SendToQueueAsync("email_queue", email);
                break;
            case PushNotification pushNotification:
                await SendToQueueAsync("push_notification_queue", pushNotification);
                break;
            default:
                throw new ArgumentException("The message parameter type is not valid.");
        }
    }

    private async Task SendToQueueAsync(string queue, object message)
    {
        var factory = new ConnectionFactory
        {
            HostName = _config.EventBusConnection,
            UserName = _config.EventBusUserName,
            Password = _config.EventBusPassword
        };

        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        var props = new BasicProperties();
        props.Persistent = true;

        await channel.QueueDeclareAsync(queue: queue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        byte[] body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

        await channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: queue,
            mandatory: true,
            basicProperties: props,
            body: body);
    }
}
