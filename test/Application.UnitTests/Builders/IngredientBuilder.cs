using PersonalAssistant.Application.Contracts.CookingAssistant.Ingredients.Models;

namespace PersonalAssistant.Application.UnitTests.Builders
{
    public class IngredientBuilder
    {
        private string name;
        private int? taskId;
        private decimal? price;

        public IngredientBuilder()
        {
            name = "Dummy name";
        }

        public IngredientBuilder WithName(string newName)
        {
            name = newName;
            return this;
        }

        public IngredientBuilder WithTaskId()
        {
            taskId = 1;
            return this;
        }

        public IngredientBuilder WithPrice(double newPrice)
        {
            price = (decimal)newPrice;
            return this;
        }

        public UpdateIngredient BuildUpdateModel()
        {
            return new UpdateIngredient
            {
                Name = name,
                TaskId = taskId,
                PriceData = new IngredientPriceData { Price = price }
            };
        }
    }
}
