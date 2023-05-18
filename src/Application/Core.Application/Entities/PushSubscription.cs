namespace Core.Application.Entities;

public class PushSubscription
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Application { get; set; }
    public string Endpoint { get; set; }
    public string AuthKey { get; set; }
    public string P256dhKey { get; set; }
    public DateTime CreatedDate { get; set; }
}
