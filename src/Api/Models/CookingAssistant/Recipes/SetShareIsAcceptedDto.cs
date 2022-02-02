namespace Api.Models.CookingAssistant.Recipes;

public class SetShareIsAcceptedDto
{
    public int RecipeId { get; set; }
    public bool IsAccepted { get; set; }
}