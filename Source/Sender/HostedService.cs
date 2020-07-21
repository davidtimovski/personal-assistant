using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PersonalAssistant.Application.Contracts.Common;
using PersonalAssistant.Sender.Contracts;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SendGrid;
using SendGrid.Helpers.Mail;
using WebPush;

namespace PersonalAssistant.Sender
{
    public sealed class HostedService : IHostedService, IDisposable
    {
        private readonly ILogger<HostedService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IPushSubscriptionsRepository _pushSubscriptionsRepository;

        public HostedService(
            ILogger<HostedService> logger,
            IConfiguration configuration,
            IPushSubscriptionsRepository pushSubscriptionsRepository)
        {
            _logger = logger;
            _configuration = configuration;
            _pushSubscriptionsRepository = pushSubscriptionsRepository;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var factory = new ConnectionFactory();

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
            async void received(object model, BasicDeliverEventArgs ea)
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body.ToArray());
                var email = JsonConvert.DeserializeObject<Email>(message);

                var client = new SendGridClient(_configuration["SendGridApiKey"]);
                var from = new EmailAddress(_configuration["SystemEmail"], _configuration["ApplicationName"]);
                var to = new EmailAddress(email.ToAddress, email.ToName);
                var emailMessage = MailHelper.CreateSingleEmail(from, to, email.Subject, email.BodyText, email.BodyHtml);

                try
                {
                    await client.SendEmailAsync(emailMessage);
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

            SetupQueue("email_queue", received, channel);
        }

        private void SetupPushNotificationQueue(IModel channel)
        {
            async void received(object model, BasicDeliverEventArgs ea)
            {
                var body = ea.Body;
                string message = Encoding.UTF8.GetString(body.ToArray());
                var pushNotification = JsonConvert.DeserializeObject<PushNotification>(message);

                var recipientSubs = await _pushSubscriptionsRepository.GetAllAsync(pushNotification.UserId, pushNotification.Application);
                string appVapidConfigPrefix = pushNotification.Application.Replace(" ", string.Empty, StringComparison.Ordinal);

                try
                {
                    foreach (var recipientSub in recipientSubs)
                    {
                        var subscription = new PushSubscription(recipientSub.Endpoint, recipientSub.P256dhKey, recipientSub.AuthKey);
                        var vapidDetails = new VapidDetails(
                            subject: "mailto:david.timovski@gmail.com",
                            publicKey: _configuration[$"{appVapidConfigPrefix}Vapid:PublicKey"],
                            privateKey: _configuration[$"{appVapidConfigPrefix}Vapid:PrivateKey"]);

                        var webPushClient = new WebPushClient();
                        try
                        {
                            await webPushClient.SendNotificationAsync(
                                subscription,
                                JsonConvert.SerializeObject(
                                    new PushNotificationMessage(
                                        pushNotification.SenderImageUri,
                                        pushNotification.Application,
                                        pushNotification.Message,
                                        pushNotification.OpenUrl
                                    ),
                                    new JsonSerializerSettings
                                    {
                                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                                    }
                                ), vapidDetails);
                        }
                        catch (WebPushException ex) when (ex.Message == "Subscription no longer valid")
                        {
                            await _pushSubscriptionsRepository.DeleteSubscriptionAsync(recipientSub.Id);
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

            SetupQueue("push_notification_queue", received, channel);
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
}
