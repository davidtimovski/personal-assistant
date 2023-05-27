﻿using System.Text;
using System.Text.Json;
using Dapper;
using Npgsql;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Sender.Models;
using SendGrid;
using SendGrid.Helpers.Mail;
using WebPush;

namespace Sender;

public sealed class HostedService : IHostedService, IDisposable
{
    private readonly ILogger<HostedService> _logger;
    private readonly IConfiguration _configuration;

    private static JsonSerializerOptions PayloadSerializationOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public HostedService(
        ILogger<HostedService> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public Task StartAsync(CancellationToken cancellationToken)
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

        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            SetupEmailQueue(channel);
            SetupPushNotificationQueue(channel);

            Thread.Sleep(Timeout.Infinite);
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    private void SetupEmailQueue(IModel channel)
    {
        async void Received(object model, BasicDeliverEventArgs ea)
        {
            var body = ea.Body;
            var message = Encoding.UTF8.GetString(body.ToArray());
            var email = JsonSerializer.Deserialize<Email>(message);

            var client = new SendGridClient(_configuration["SendGridApiKey"]);
            var from = new EmailAddress(_configuration["SystemEmail"], "Personal Assistant");
            var to = new EmailAddress(email.ToAddress, email.ToName);
            var emailMessage = MailHelper.CreateSingleEmail(from, to, email.Subject, email.BodyText, email.BodyHtml);

            try
            {
                await client.SendEmailAsync(emailMessage);
                _logger.LogDebug($"Sending email to: {to}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Email sending failed");
                throw;
            }
            finally
            {
                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            }
        }

        SetupQueue("email_queue", Received, channel);
    }

    private void SetupPushNotificationQueue(IModel channel)
    {
        async void Received(object model, BasicDeliverEventArgs ea)
        {
            var body = ea.Body;
            string message = Encoding.UTF8.GetString(body.ToArray());
            var pushNotification = JsonSerializer.Deserialize<PushNotification>(message);

            using var conn = new NpgsqlConnection(_configuration["ConnectionString"]);
            conn.Open();

            var recipientSubs = conn.Query<Core.Application.Entities.PushSubscription>("SELECT * FROM push_subscriptions WHERE user_id = @UserId AND application = @Application",
                new { pushNotification.UserId, pushNotification.Application });

            string applicationName = pushNotification.Application.Replace(" ", string.Empty, StringComparison.Ordinal);

            var webPushClient = new WebPushClient();

            try
            {
                foreach (var recipientSub in recipientSubs)
                {
                    var subscription = new PushSubscription(recipientSub.Endpoint, recipientSub.P256dhKey, recipientSub.AuthKey);
                    var vapidDetails = new VapidDetails(
                        subject: "mailto:david.timovski@gmail.com",
                        publicKey: _configuration[$"{applicationName}:Vapid:PublicKey"],
                        privateKey: _configuration[$"{applicationName}:Vapid:PrivateKey"]);

                    try
                    {
                        var pushNotificationPayload = new PushNotificationMessage(
                            pushNotification.SenderImageUri,
                            pushNotification.Application,
                            pushNotification.Message,
                            pushNotification.OpenUrl
                        );
                        var payload = JsonSerializer.Serialize(pushNotificationPayload, PayloadSerializationOptions);

                        await webPushClient.SendNotificationAsync(subscription, payload, vapidDetails);
                    }
                    catch (WebPushException ex) when (ex.Message.StartsWith("Subscription no longer valid"))
                    {
                        conn.Execute("DELETE FROM push_subscriptions WHERE id = @Id", new { recipientSub.Id });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Push notification sending failed");
                throw;
            }
            finally
            {
                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            }
        }

        SetupQueue("push_notification_queue", Received, channel);
    }

    private static void SetupQueue(string queue, EventHandler<BasicDeliverEventArgs> received, IModel channel)
    {
        channel.QueueDeclare(queue: queue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += received;

        channel.BasicConsume(queue: queue,
            autoAck: false,
            consumer: consumer);
    }
}