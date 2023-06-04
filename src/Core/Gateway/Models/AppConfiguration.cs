using System.ComponentModel.DataAnnotations;

namespace Gateway.Models;

public class AppConfiguration
{
    /// <summary>
    /// Coming from appsettings.*.json.
    /// </summary>
    [Required]
    public AppUrls Urls { get; set; } = null!;
}

public class AppUrls
{
    [Required]
    public string ToDoAssistant { get; set; } = null!;

    [Required]
    public string CookingAssistant { get; set; } = null!;

    [Required]
    public string Accountant { get; set; } = null!;

    [Required]
    public string Weatherman { get; set; } = null!;

    [Required]
    public string Trainer { get; set; } = null!;
}
