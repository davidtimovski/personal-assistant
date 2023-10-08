namespace Chef.Application.Entities;

public class IngredientTask
{
    public int IngredientId { get; set; }
    public int UserId { get; set; }
    public int TaskId { get; set; }
    public DateTime CreatedDate { get; set; }
}
