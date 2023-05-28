namespace Core.Application.Entities;

public class PushSubscription
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Application { get; set; } = null!;
    public string Endpoint { get; set; } = null!;
    public string AuthKey { get; set; } = null!;
    public string P256dhKey { get; set; } = null!;
    public DateTime CreatedDate { get; set; }
}
