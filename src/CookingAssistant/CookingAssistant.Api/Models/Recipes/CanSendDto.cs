namespace CookingAssistant.Api.Models.Recipes;

public class CanSendDto
{
    public int UserId { get; set; }
    public string ImageUri { get; set; }
    public bool CanSend { get; set; }
    public bool AlreadySent { get; set; }
}
