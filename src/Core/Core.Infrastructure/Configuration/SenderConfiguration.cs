using System.ComponentModel.DataAnnotations;

namespace Core.Infrastructure.Configuration;

public class SenderConfiguration
{
    [Required]
    public string EventBusConnection { get; set; } = null!;

    [Required]
    public string EventBusUserName { get; set; } = null!;

    [Required]
    public string EventBusPassword { get; set; } = null!;
}
