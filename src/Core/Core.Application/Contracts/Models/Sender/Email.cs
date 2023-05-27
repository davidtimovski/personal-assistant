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

    public string ToAddress { get; set; }
    public string ToName { get; set; }
    public string Subject { get; set; }
    public string BodyText { get; set; }
    public string BodyHtml { get; set; }
}
