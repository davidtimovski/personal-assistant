﻿using System.Reflection;
using System.Text;
using Application.Contracts;
using Infrastructure.Sender.Models;
using Microsoft.Extensions.Localization;

namespace Account.Services;

public interface IEmailTemplateService
{
    Task EnqueueRegisterConfirmationEmailAsync(string toName, string toEmail, Uri url, string language);
    Task EnqueueNewRegistrationEmailAsync(string name, string email);
    Task EnqueueNewEmailVerificationEmailAsync(string name, string email);
}

public class EmailTemplateService : IEmailTemplateService
{
    private readonly ISenderService _senderService;
    private readonly IWebHostEnvironment _env;
    private readonly IStringLocalizer<EmailTemplateService> _localizer;
    private readonly string _adminEmail;

    public EmailTemplateService(
        ISenderService senderService,
        IWebHostEnvironment env,
        IConfiguration configuration,
        IStringLocalizer<EmailTemplateService> localizer)
    {
        _senderService = senderService;
        _env = env;
        _localizer = localizer;
        _adminEmail = configuration["AdminEmail"];
    }

    public async Task EnqueueRegisterConfirmationEmailAsync(string toName, string toEmail, Uri url, string language)
    {
        var email = new Email
        {
            ToAddress = toEmail,
            ToName = toName,
            Subject = _localizer["RegisterConfirmationEmailSubject"]
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

    public async Task EnqueueNewRegistrationEmailAsync(string newUserName, string newUserEmail)
    {
        if (_env.EnvironmentName == Environments.Production)
        {
            var email = new Email
            {
                ToAddress = _adminEmail,
                ToName = "Admin",
                Subject = "New registration on Personal Assistant"
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
        if (_env.EnvironmentName == Environments.Production)
        {
            var email = new Email
            {
                ToAddress = _adminEmail,
                ToName = "Admin",
                Subject = "New email confirmation on Personal Assistant"
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
        var resourceStream = assembly.GetManifestResourceStream($"Account.Resources.EmailTemplates.{language.Replace('-', '_')}.{templateName}");

        using var reader = new StreamReader(resourceStream, Encoding.UTF8);
        return await reader.ReadToEndAsync();
    }
}
