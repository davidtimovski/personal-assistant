namespace Domain.Entities.CookingAssistant;

public class IngredientTask : Entity
{
    public int IngredientId { get; set; }
    public int UserId { get; set; }
    public int TaskId { get; set; }
}
