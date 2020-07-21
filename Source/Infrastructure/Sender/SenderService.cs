using System;
using System.Text;
using Newtonsoft.Json;
using PersonalAssistant.Application.Contracts.Common;
using PersonalAssistant.Infrastructure.Sender.Models;
using RabbitMQ.Client;

namespace PersonalAssistant.Infrastructure.Sender
{
    public class SenderService : ISenderService
    {
        public void Enqueue<T>(T message)
        {
            if (message is Email email)
            {
                SendToQueue("email_queue", email);
            }
            else if (message is PushNotification pushNotification)
            {
                SendToQueue("push_notification_queue", pushNotification);
            }
            else
            {
                throw new ArgumentException("The message parameter type is not valid.");
            }
        }

        private void SendToQueue(string queue, object message)
        {
            var factory = new ConnectionFactory();

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
}
