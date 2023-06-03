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

    public void Enqueue(ISendable sendable)
    {
        switch (sendable)
        {
            case Email email:
                SendToQueue("email_queue", email);
                break;
            case PushNotification pushNotification:
                SendToQueue("push_notification_queue", pushNotification);
                break;
            default:
                throw new ArgumentException("The message parameter type is not valid.");
        }
    }

    private void SendToQueue(string queue, object message)
    {
        var factory = new ConnectionFactory
        {
            HostName = _config.EventBusConnection,
            UserName = _config.EventBusUserName,
            Password = _config.EventBusPassword
        };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        var properties = channel.CreateBasicProperties();
        properties.Persistent = true;

        channel.QueueDeclare(queue: queue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        byte[] body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

        channel.BasicPublish(exchange: string.Empty,
            routingKey: queue,
            basicProperties: properties,
            body: body);
    }
}
