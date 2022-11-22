namespace ToDoAssistant.Api.Models;

public class CanShareVm
{
    public int UserId { get; set; }
    public string ImageUri { get; set; }
    public bool CanShare { get; set; }
}
