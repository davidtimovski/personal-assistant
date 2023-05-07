namespace Sender.Contracts;

internal class Email
{
    public string ToAddress { get; set; }
    public string ToName { get; set; }
    public string Subject { get; set; }
    public string BodyText { get; set; }
    public string BodyHtml { get; set; }
}
