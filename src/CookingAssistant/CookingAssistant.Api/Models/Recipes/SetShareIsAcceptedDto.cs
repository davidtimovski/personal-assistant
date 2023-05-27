namespace CookingAssistant.Api.Models.Recipes;

public class SetShareIsAcceptedDto
{
    public int RecipeId { get; set; }
    public bool IsAccepted { get; set; }
}
