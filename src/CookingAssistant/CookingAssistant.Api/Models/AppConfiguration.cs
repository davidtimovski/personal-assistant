using System.ComponentModel.DataAnnotations;
using Cdn.Configuration;
using CookingAssistant.Application.Contracts.DietaryProfiles.Models;
using Core.Infrastructure.Configuration;

namespace CookingAssistant.Api.Models;

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
    public DietaryProfileConfig DietaryProfile { get; set; } = null!;

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

public class DietaryProfileConfig
{
    [Required]
    public Dictionary<string, float> ActivityMultiplier { get; set; } = null!;

    [Required]
    public Dictionary<string, short> DietaryGoalCalories { get; set; } = null!;

    [Required]
    public DailyIntakeReference ReferenceDailyIntake { get; set; } = null!;
}
