using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using Core.Application.Utils;
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
    private static readonly JsonSerializerOptions PayloadSerializationOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly AppConfiguration _config;
    private readonly IReadOnlyDictionary<string, VapidConfiguration> _vapidConfig;
    private readonly ILogger<HostedService> _logger;

    public HostedService(
        IOptions<AppConfiguration>? config,
        ILogger<HostedService>? logger)
    {
        _config = ArgValidator.NotNull(config).Value;
        _vapidConfig = new Dictionary<string, VapidConfiguration>
        {
            { "To Do Assistant", _config.Sender.ToDoAssistantVapid },
            { "Chef", _config.Sender.ChefVapid },
        };
        _logger = ArgValidator.NotNull(logger);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _config.RabbitMQ.EventBusConnection,
            UserName = _config.RabbitMQ.EventBusUserName,
            Password = _config.RabbitMQ.EventBusPassword
        };

        using (var connection = await factory.CreateConnectionAsync())
        using (var channel = await connection.CreateChannelAsync())
        {
            await SetupEmailQueueAsync(channel);
            await SetupPushNotificationQueueAsync(channel);

            Thread.Sleep(Timeout.Infinite);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    private async Task SetupEmailQueueAsync(IChannel channel)
    {
        async Task ReceivedAsync(object? model, BasicDeliverEventArgs ea)
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
                await channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
            }
        }

        await SetupQueueAsync("email_queue", ReceivedAsync, channel);
    }

    private async Task SetupPushNotificationQueueAsync(IChannel channel)
    {
        async Task ReceivedAsync(object? model, BasicDeliverEventArgs ea)
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
                        var payload = new PushNotificationPayload(
                            pushNotification.SenderImageUri,
                            pushNotification.Application,
                            pushNotification.Message,
                            pushNotification.OpenUrl
                        );
                        var payloadString = JsonSerializer.Serialize(payload, PayloadSerializationOptions);

                        await webPushClient.SendNotificationAsync(subscription, payloadString, vapidDetails);
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
                await channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
                webPushClient.Dispose();
            }
        }

        await SetupQueueAsync("push_notification_queue", ReceivedAsync, channel);
    }

    private static async Task SetupQueueAsync(string queue, AsyncEventHandler<BasicDeliverEventArgs> received, IChannel channel)
    {
        await channel.QueueDeclareAsync(queue: queue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += received;

        await channel.BasicConsumeAsync(queue: queue,
            autoAck: false,
            consumer: consumer);
    }
}
