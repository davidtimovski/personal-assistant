using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Auth.Models.Config;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using PersonalAssistant.Application.Contracts.Common;
using PersonalAssistant.Infrastructure.Sender.Models;

namespace Auth.Services
{
    public interface IEmailTemplateService
    {
        Task EnqueueRegisterConfirmationEmailAsync(string toName, string toEmail, Uri url, string language);
        Task EnqueuePasswordResetEmailAsync(string toEmail, Uri url, string language);
        Task EnqueueNewRegistrationEmailAsync(string name, string email);
        Task EnqueueNewEmailVerificationEmailAsync(string name, string email);
    }

    public class EmailTemplateService : IEmailTemplateService
    {
        private readonly ISenderService _senderService;
        private readonly IWebHostEnvironment _env;
        private readonly AppSettings _appSettings;
        private readonly IStringLocalizer<EmailTemplateService> _localizer;

        public EmailTemplateService(
            ISenderService senderService,
            IWebHostEnvironment env,
            IOptions<AppSettings> appSettingsOptions,
            IStringLocalizer<EmailTemplateService> localizer)
        {
            _senderService = senderService;
            _env = env;
            _appSettings = appSettingsOptions.Value;
            _localizer = localizer;
        }

        public async Task EnqueueRegisterConfirmationEmailAsync(string toName, string toEmail, Uri url, string language)
        {
            var email = new Email
            {
                ToAddress = toEmail,
                ToName = toName,
                Subject = _localizer["RegisterConfirmationEmailSubject", _appSettings.ApplicationName]
            };

            string bodyText = await GetTemplateContentsAsync("RegisterConfirmation.txt", language);
            email.BodyText = bodyText
                .Replace("#NAME#", toName, StringComparison.Ordinal)
                .Replace("#URL#", url.AbsoluteUri, StringComparison.Ordinal);

            string bodyHtml = await GetTemplateContentsAsync("RegisterConfirmation.html", language);
            email.BodyHtml = bodyHtml
                .Replace("#SUBJECT#", email.Subject, StringComparison.Ordinal)
                .Replace("#NAME#", toName, StringComparison.Ordinal)
                .Replace("#URL#", url.AbsoluteUri, StringComparison.Ordinal);

            _senderService.Enqueue(email);
        }

        public async Task EnqueuePasswordResetEmailAsync(string toEmail, Uri url, string language)
        {
            var email = new Email
            {
                ToAddress = toEmail,
                ToName = string.Empty,
                Subject = _localizer["PasswordResetEmailSubject", _appSettings.ApplicationName]
            };

            string bodyText = await GetTemplateContentsAsync("PasswordReset.txt", language);
            email.BodyText = bodyText
                .Replace("#URL#", url.AbsoluteUri, StringComparison.Ordinal);

            string bodyHtml = await GetTemplateContentsAsync("PasswordReset.html", language);
            email.BodyHtml = bodyHtml
                .Replace("#SUBJECT#", email.Subject, StringComparison.Ordinal)
                .Replace("#URL#", url.AbsoluteUri, StringComparison.Ordinal);

            _senderService.Enqueue(email);
        }

        public async Task EnqueueNewRegistrationEmailAsync(string newUserName, string newUserEmail)
        {
            if (_env.EnvironmentName == "Production")
            {
                var email = new Email
                {
                    ToAddress = _appSettings.AdminEmail,
                    ToName = "Admin",
                    Subject = $"New registration on {_appSettings.ApplicationName}"
                };

                string bodyText = await GetTemplateContentsAsync("NewRegistration.txt", "admin");
                email.BodyText = bodyText
                    .Replace("#NAME#", newUserName, StringComparison.Ordinal)
                    .Replace("#EMAIL#", newUserEmail, StringComparison.Ordinal);

                string bodyHtml = await GetTemplateContentsAsync("NewRegistration.html", "admin");
                email.BodyHtml = bodyHtml
                    .Replace("#SUBJECT#", email.Subject, StringComparison.Ordinal)
                    .Replace("#NAME#", newUserName, StringComparison.Ordinal)
                    .Replace("#EMAIL#", newUserEmail, StringComparison.Ordinal);

                _senderService.Enqueue(email);
            }
        }

        public async Task EnqueueNewEmailVerificationEmailAsync(string newUserName, string newUserEmail)
        {
            if (_env.EnvironmentName == "Production")
            {
                var email = new Email
                {
                    ToAddress = _appSettings.AdminEmail,
                    ToName = "Admin",
                    Subject = $"New email confirmation on {_appSettings.ApplicationName}"
                };

                string bodyText = await GetTemplateContentsAsync("NewEmailVerification.txt", "admin");
                email.BodyText = bodyText
                    .Replace("#NAME#", newUserName, StringComparison.Ordinal)
                    .Replace("#EMAIL#", newUserEmail, StringComparison.Ordinal);

                string bodyHtml = await GetTemplateContentsAsync("NewEmailVerification.html", "admin");
                email.BodyHtml = bodyHtml
                    .Replace("#SUBJECT#", email.Subject, StringComparison.Ordinal)
                    .Replace("#NAME#", newUserName, StringComparison.Ordinal)
                    .Replace("#EMAIL#", newUserEmail, StringComparison.Ordinal);

                _senderService.Enqueue(email);
            }
        }

        private static async Task<string> GetTemplateContentsAsync(string templateName, string language)
        {
            var assembly = Assembly.GetEntryAssembly();
            var resourceStream = assembly.GetManifestResourceStream($"PersonalAssistant.Auth.Resources.EmailTemplates.{language.Replace('-', '_')}.{templateName}");

            using var reader = new StreamReader(resourceStream, Encoding.UTF8);
            return await reader.ReadToEndAsync();
        }
    }
}
