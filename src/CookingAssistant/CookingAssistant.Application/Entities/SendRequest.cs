using Core.Application;
using Core.Application.Entities;

namespace CookingAssistant.Application.Entities;

public class SendRequest : Entity
{
    public int RecipeId { get; set; }
    public int UserId { get; set; }
    public bool IsDeclined { get; set; }

    public Recipe Recipe { get; set; }
    public User User { get; set; }
}
