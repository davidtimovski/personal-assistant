namespace PersonalAssistant.Application.Contracts.CookingAssistant.Recipes.Models
{
    public class UpdateRecipeIngredient
    {
        public string Name { get; set; }
        public int? TaskId { get; set; }
        public float? Amount { get; set; }
        public string Unit { get; set; }
    }
}
