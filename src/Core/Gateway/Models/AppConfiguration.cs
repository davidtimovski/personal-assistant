using System.ComponentModel.DataAnnotations;

namespace Gateway.Models;

public class AppConfiguration
{
    /// <summary>
    /// Coming from appsettings.*.json.
    /// </summary>
    [Required]
    public required AppUrls Urls { get; set; }
}

public class AppUrls
{
    [Required]
    public required string ToDoAssistant { get; set; }

    [Required]
    public required string Chef { get; set; }

    [Required]
    public required string Accountant { get; set; }

    [Required]
    public required string Weatherman { get; set; }

    [Required]
    public required string Trainer { get; set; }
}
