using System.ComponentModel.DataAnnotations;
using Cdn.Configuration;
using Core.Infrastructure.Configuration;

namespace ToDoAssistant.Api.Models;

public class AppConfiguration
{
#if !DEBUG
    [Required]
#endif
    public KeyVaultConfiguration KeyVault { get; set; } = null!;

    [Required]
    public string ConnectionString { get; set; } = null!;

    [Required]
    public Auth0Config Auth0 { get; set; } = null!;

    [Required]
    public string Url { get; set; } = null!;

    [Required]
    public CloudinaryConfig Cloudinary { get; set; } = null!;

    [Required]
    public SenderConfiguration RabbitMQ { get; set; } = null!;
}

public class Auth0Config
{
    [Required]
    public string Domain { get; set; } = null!;

    [Required]
    public string Audience { get; set; } = null!;
}
