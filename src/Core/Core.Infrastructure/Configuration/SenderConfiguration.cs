using System.ComponentModel.DataAnnotations;

namespace Core.Infrastructure.Configuration;

public class SenderConfiguration
{
    [Required]
    public required string EventBusConnection { get; set; }

    [Required]
    public required string EventBusUserName { get; set; }

    [Required]
    public required string EventBusPassword { get; set; }
}
