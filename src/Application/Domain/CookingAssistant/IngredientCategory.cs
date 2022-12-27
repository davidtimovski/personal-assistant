namespace Application.Domain.CookingAssistant;

public class IngredientCategory : Entity
{
    public int Id { get; set; }
    public int? ParentId { get; set; }
    public string Name { get; set; }

    public IngredientCategory Parent { get; set; }
}
