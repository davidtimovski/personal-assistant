namespace CookingAssistant.Application.Entities;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
    public string Language { get; set; }
    public bool CookingNotificationsEnabled { get; set; }
    public bool ImperialSystem { get; set; }
    public string ImageUri { get; set; }

    public DietaryProfile DietaryProfile { get; set; }
}
