using Application.Contracts.CookingAssistant.Ingredients.Models;

namespace Application.UnitTests.Builders
{
    public class IngredientBuilder
    {
        private string name;
        private int? taskId;
 
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

        public UpdateIngredient BuildUpdateModel()
        {
            return new UpdateIngredient
            {
                Name = name,
                TaskId = taskId,
                PriceData = new IngredientPriceData()
            };
        }
    }
}
