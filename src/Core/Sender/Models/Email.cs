namespace Sender.Models;

internal class Email
{
    internal string ToAddress { get; set; } = null!;
    internal string ToName { get; set; } = null!;
    internal string Subject { get; set; } = null!;
    internal string BodyText { get; set; } = null!;
    internal string BodyHtml { get; set; } = null!;
}
