using System.ComponentModel.DataAnnotations;

namespace Sender;

public class SenderConfiguration
{
    [Required]
    public string EventBusConnection { get; set; } = null!;

    [Required]
    public string EventBusUserName { get; set; } = null!;

    [Required]
    public string EventBusPassword { get; set; } = null!;

    [Required]
    public string SendGridApiKey { get; set; } = null!;

    [Required]
    public string SystemEmail { get; set; } = null!;

    [Required]
    public string ConnectionString { get; set; } = null!;

    [Required]
    public VapidConfiguration ToDoAssistantVapid { get; set; } = null!;

    [Required]
    public VapidConfiguration CookingAssistantVapid { get; set; } = null!;
}

public class VapidConfiguration
{
    [Required]
    public string PublicKey { get; set; } = null!;

    [Required]
    public string PrivateKey { get; set; } = null!;
}
