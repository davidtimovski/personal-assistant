using System.ComponentModel.DataAnnotations;

namespace Gateway.Models;

public sealed class AppConfiguration
{
    /// <summary>
    /// Coming from appsettings.*.json.
    /// </summary>
    [Required]
    public required AppUrls Urls { get; init; }
}

public sealed class AppUrls
{
    [Required]
    public required string ToDoAssistant { get; init; }

    [Required]
    public required string Chef { get; init; }

    [Required]
    public required string Accountant { get; init; }

    [Required]
    public required string Weatherman { get; init; }

    [Required]
    public required string Trainer { get; init; }
}
