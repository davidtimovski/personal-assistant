using System;
using System.Text;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Application.Contracts.Common;
using Infrastructure.Sender.Models;
using RabbitMQ.Client;

namespace Infrastructure.Sender;

public class SenderService : ISenderService
{
    private readonly IConfiguration _configuration;
    public SenderService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void Enqueue<T>(T message)
    {
        switch (message)
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
            HostName = _configuration["EventBusConnection"]
        };

        if (!string.IsNullOrEmpty(_configuration["EventBusUserName"]))
        {
            factory.UserName = _configuration["EventBusUserName"];
        }

        if (!string.IsNullOrEmpty(_configuration["EventBusPassword"]))
        {
            factory.Password = _configuration["EventBusPassword"];
        }

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        var properties = channel.CreateBasicProperties();
        properties.Persistent = true;

        channel.QueueDeclare(queue: queue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        byte[] body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

        channel.BasicPublish(exchange: string.Empty,
            routingKey: queue,
            basicProperties: properties,
            body: body);
    }
}
