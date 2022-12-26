using System.Reflection;
using System.Text;
using Core.Application.Contracts;
using Infrastructure.Sender.Models;
using Microsoft.Extensions.Localization;

namespace Account.Services;

public interface IEmailTemplateService
{
    Task EnqueueNewRegistrationEmailAsync(string name, string email);
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

    private static async Task<string> GetTemplateContentsAsync(string templateName, string language)
    {
        var assembly = Assembly.GetEntryAssembly();
        var resourceStream = assembly.GetManifestResourceStream($"Account.Resources.EmailTemplates.{language.Replace('-', '_')}.{templateName}");

        using var reader = new StreamReader(resourceStream, Encoding.UTF8);
        return await reader.ReadToEndAsync();
    }
}
