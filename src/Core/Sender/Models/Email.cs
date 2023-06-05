using System.Text.Json.Serialization;

namespace Sender.Models;

public class Email
{
    [JsonRequired]
    public string ToAddress { get; set; } = null!;

    [JsonRequired]
    public string ToName { get; set; } = null!;

    [JsonRequired]
    public string Subject { get; set; } = null!;

    [JsonRequired]
    public string BodyText { get; set; } = null!;

    [JsonRequired]
    public string BodyHtml { get; set; } = null!;
}
