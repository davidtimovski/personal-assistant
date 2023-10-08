namespace ToDoAssistant.Api.Models.Lists.Responses;

public class CanShareResponse
{
    public int UserId { get; set; }
    public string ImageUri { get; set; } = null!;
    public bool CanShare { get; set; }
}
