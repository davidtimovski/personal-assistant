namespace PersonalAssistant.Application.Contracts.CookingAssistant.Ingredients.Models
{
    public class IngredientPriceData
    {
        public bool IsSet { get; set; }
        public short ProductSize { get; set; }
        public bool ProductSizeIsOneUnit { get; set; }
        public decimal? Price { get; set; }
        public string Currency { get; set; }
    }
}
