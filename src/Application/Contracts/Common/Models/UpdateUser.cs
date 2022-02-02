namespace Application.Contracts.Common.Models;

public class UpdateUser
{
    public bool ToDoNotificationsEnabled { get; set; }
    public bool CookingNotificationsEnabled { get; set; }
    public bool ImperialSystem { get; set; }
}