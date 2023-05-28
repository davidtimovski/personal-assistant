using System.Reflection;
using System.Text;
using Core.Application.Contracts;
using Core.Application.Contracts.Models.Sender;
using Microsoft.Extensions.Localization;

namespace Account.Web.Services;

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
        if (_env.EnvironmentName != Environments.Production)
        {
            return;
        }

        string bodyTextTemplate = await GetTemplateContentsAsync("NewRegistration.txt", "admin");
        var bodyText = bodyTextTemplate
            .Replace("#NAME#", newUserName, StringComparison.Ordinal)
            .Replace("#EMAIL#", newUserEmail, StringComparison.Ordinal);

        const string subject = "New registration on Personal Assistant";
        string bodyHtmlTemplate = await GetTemplateContentsAsync("NewRegistration.html", "admin");
        var bodyHtml = bodyHtmlTemplate
            .Replace("#SUBJECT#", subject, StringComparison.Ordinal)
            .Replace("#NAME#", newUserName, StringComparison.Ordinal)
            .Replace("#EMAIL#", newUserEmail, StringComparison.Ordinal);

        var email = new Email(_adminEmail, "Admin", subject, bodyText, bodyHtml);

        _senderService.Enqueue(email);
    }

    private static async Task<string> GetTemplateContentsAsync(string templateName, string language)
    {
        var assembly = Assembly.GetEntryAssembly();
        if (assembly is null)
        {
            throw new Exception("Could not get entry assembly");
        }

        var resourceStream = assembly.GetManifestResourceStream($"Account.Resources.EmailTemplates.{language.Replace('-', '_')}.{templateName}");
        if (resourceStream is null)
        {
            throw new Exception("Could not get manifest resource stream");
        }

        using var reader = new StreamReader(resourceStream, Encoding.UTF8);
        return await reader.ReadToEndAsync();
    }
}
