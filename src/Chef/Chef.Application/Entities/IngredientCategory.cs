using Core.Application;

namespace Chef.Application.Entities;

public class IngredientCategory : Entity
{
    public int Id { get; set; }
    public int? ParentId { get; set; }
    public string Name { get; set; } = null!;

    public IngredientCategory? Parent { get; set; }
}
