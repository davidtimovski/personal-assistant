using Core.Application;

namespace Chef.Application.Entities;

public class IngredientBrand : Entity
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
}
