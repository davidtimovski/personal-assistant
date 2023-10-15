namespace Core.Application.Contracts.Models.Sender;

public class Email : ISendable
{
    public Email(string toAddress, string toName, string subject, string bodyText, string bodyHtml)
    {
        ToAddress = toAddress;
        ToName = toName;
        Subject = subject;
        BodyText = bodyText;
        BodyHtml = bodyHtml;
    }

    public string ToAddress { get; init; }
    public string ToName { get; init; }
    public string Subject { get; init; }
    public string BodyText { get; init; }
    public string BodyHtml { get; init; }
}
