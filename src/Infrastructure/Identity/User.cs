namespace Infrastructure.Identity;

public class User
{
    public string Email { get; set; }
    public string Name { get; set; }
    public string Language { get; set; }
    public bool ToDoNotificationsEnabled { get; set; }
    public bool CookingNotificationsEnabled { get; set; }
    public string ImageUri { get; set; }
}
