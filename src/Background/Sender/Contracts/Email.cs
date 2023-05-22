namespace Sender.Contracts;

internal class Email
{
    internal string ToAddress { get; set; }
    internal string ToName { get; set; }
    internal string Subject { get; set; }
    internal string BodyText { get; set; }
    internal string BodyHtml { get; set; }
}
