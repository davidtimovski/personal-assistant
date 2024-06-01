namespace Core.Application.Entities;

public class Friendship
{
    public int SenderId { get; set; }
    public int RecipientId { get; set; }
    public bool? IsAccepted { get; set; }
    public string[] Permissions { get; set; } = null!;
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }

    public User? Sender { get; set; }
    public User? Recipient { get; set; }
}
