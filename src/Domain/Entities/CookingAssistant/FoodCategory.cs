namespace Domain.Entities.CookingAssistant;

public class FoodCategory : Entity
{
    public int Id { get; set; }
    public int ParentId { get; set; }
    public string Name { get; set; }

    public FoodCategory ParentCategory { get; set; }
}