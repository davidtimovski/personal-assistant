namespace ToDoAssistant.Api.Models;

public class CanShareResponse
{
    public int UserId { get; set; }
    public string ImageUri { get; set; } = null!;
    public bool CanShare { get; set; }
}
