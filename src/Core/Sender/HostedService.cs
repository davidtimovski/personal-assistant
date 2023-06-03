using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using Dapper;
using Microsoft.Extensions.Options;
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
    private readonly AppConfiguration _config;
    private readonly IReadOnlyDictionary<string, VapidConfiguration> _vapidConfig;
    private readonly ILogger<HostedService> _logger;

    private static JsonSerializerOptions PayloadSerializationOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public HostedService(
        IOptions<AppConfiguration> config,
        ILogger<HostedService> logger)
    {
        _config = config.Value;
        _vapidConfig = new Dictionary<string, VapidConfiguration>
        {
            { "To Do Assistant", _config.ToDoAssistantVapid },
            { "Cooking Assistant", _config.CookingAssistantVapid },
        };
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _config.EventBusConnection,
            UserName = _config.EventBusUserName,
            Password = _config.EventBusPassword
        };

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
        async void Received(object? model, BasicDeliverEventArgs ea)
        {
            var body = ea.Body;
            var emailJson = Encoding.UTF8.GetString(body.ToArray());

            var email = JsonSerializer.Deserialize<Email>(emailJson);
            if (email is null)
            {
                throw new SerializationException("Email JSON could not be deserialized");
            }

            var client = new SendGridClient(_config.SendGridApiKey);
            var from = new EmailAddress(_config.SystemEmail, "Personal Assistant");
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
        async void Received(object? model, BasicDeliverEventArgs ea)
        {
            var body = ea.Body;
            string notificationJson = Encoding.UTF8.GetString(body.ToArray());

            var pushNotification = JsonSerializer.Deserialize<PushNotification>(notificationJson);
            if (pushNotification is null)
            {
                throw new SerializationException("Push notification JSON could not be deserialized");
            }

            using var conn = new NpgsqlConnection(_config.ConnectionString);
            conn.Open();

            var recipientSubs = conn.Query<Core.Application.Entities.PushSubscription>("SELECT * FROM push_subscriptions WHERE user_id = @UserId AND application = @Application",
                new { pushNotification.UserId, pushNotification.Application });

            var webPushClient = new WebPushClient();

            try
            {
                foreach (var recipientSub in recipientSubs)
                {
                    var subscription = new PushSubscription(recipientSub.Endpoint, recipientSub.P256dhKey, recipientSub.AuthKey);
                    var appVapidConfig = _vapidConfig[pushNotification.Application];

                    var vapidDetails = new VapidDetails(
                        subject: "mailto:david.timovski@gmail.com",
                        publicKey: appVapidConfig.PublicKey,
                        privateKey: appVapidConfig.PrivateKey);

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
                webPushClient.Dispose();
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
