using FluentValidation;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Tasks;

namespace PersonalAssistant.Application.Contracts.CookingAssistant.Ingredients.Models
{
    public class UpdateIngredient
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int? TaskId { get; set; }
        public string Name { get; set; }
        public IngredientNutritionData NutritionData { get; set; } = new IngredientNutritionData();
        public IngredientPriceData PriceData { get; set; } = new IngredientPriceData();
    }

    public class UpdateIngredientValidator : AbstractValidator<UpdateIngredient>
    {
        public UpdateIngredientValidator(IIngredientService ingredientService, ITaskService taskService)
        {
            RuleFor(dto => dto.UserId)
                .NotEmpty().WithMessage("Unauthorized")
                .MustAsync(async (dto, userId, val) => await ingredientService.ExistsAsync(dto.Id, userId)).WithMessage("Unauthorized")
                .MustAsync(async (dto, userId, val) => !await ingredientService.ExistsAsync(dto.Id, dto.Name, userId)).WithMessage("AlreadyExists");

            RuleFor(dto => dto.TaskId)
                .MustAsync(async (dto, taskId, val) => !taskId.HasValue || await taskService.ExistsAsync(taskId.Value, dto.UserId)).WithMessage("IsLinkedToNonExistentTask");

            RuleFor(dto => dto.Name).Must((dto, name) =>
            {
                return dto.TaskId.HasValue ? true : !string.IsNullOrWhiteSpace(name);
            }).WithMessage("Ingredients.UpdateIngredient.NameIsRequired")
                .MaximumLength(50).WithMessage("Ingredients.UpdateIngredient.NameMaxLength");

            RuleFor(dto => dto.NutritionData.ServingSize).Must((dto, servingSize) =>
            {
                return servingSize != 0 || dto.NutritionData.ServingSizeIsOneUnit;
            });
            RuleFor(dto => dto.NutritionData.Calories).Must(calories =>
            {
                return !calories.HasValue || (calories >= 0 && calories < 1000);
            });
            RuleFor(dto => dto.NutritionData.Fat).Must(fat =>
            {
                return !fat.HasValue || (fat >= 0 && fat < 1000);
            });
            RuleFor(dto => dto.NutritionData.SaturatedFat).Must(saturatedFat =>
            {
                return !saturatedFat.HasValue || (saturatedFat >= 0 && saturatedFat < 1000);
            });
            RuleFor(dto => dto.NutritionData.TransFat).Must(transFat =>
            {
                return !transFat.HasValue || (transFat >= 0 && transFat < 1000);
            });
            RuleFor(dto => dto.NutritionData.Carbohydrate).Must(carbohydrate =>
            {
                return !carbohydrate.HasValue || (carbohydrate >= 0 && carbohydrate < 1000);
            });
            RuleFor(dto => dto.NutritionData.Sugars).Must(sugars =>
            {
                return !sugars.HasValue || (sugars >= 0 && sugars < 1000);
            });
            RuleFor(dto => dto.NutritionData.AddedSugars).Must(addedSugars =>
            {
                return !addedSugars.HasValue || (addedSugars >= 0 && addedSugars < 1000);
            });
            RuleFor(dto => dto.NutritionData.Fiber).Must(fiber =>
            {
                return !fiber.HasValue || (fiber >= 0 && fiber < 1000);
            });
            RuleFor(dto => dto.NutritionData.Protein).Must(protein =>
            {
                return !protein.HasValue || (protein >= 0 && protein < 1000);
            });
            RuleFor(dto => dto.NutritionData.Sodium).Must(sodium =>
            {
                return !sodium.HasValue || (sodium >= 0 && sodium < 20000);
            });
            RuleFor(dto => dto.NutritionData.Cholesterol).Must(cholesterol =>
            {
                return !cholesterol.HasValue || (cholesterol >= 0 && cholesterol < 20000);
            });
            RuleFor(dto => dto.NutritionData.VitaminA).Must(vitaminA =>
            {
                return !vitaminA.HasValue || (vitaminA >= 0 && vitaminA < 20000);
            });
            RuleFor(dto => dto.NutritionData.VitaminC).Must(vitaminC =>
            {
                return !vitaminC.HasValue || (vitaminC >= 0 && vitaminC < 20000);
            });
            RuleFor(dto => dto.NutritionData.VitaminD).Must(vitaminD =>
            {
                return !vitaminD.HasValue || (vitaminD >= 0 && vitaminD < 20000);
            });
            RuleFor(dto => dto.NutritionData.Calcium).Must(calcium =>
            {
                return !calcium.HasValue || (calcium >= 0 && calcium < 20000);
            });
            RuleFor(dto => dto.NutritionData.Iron).Must(iron =>
            {
                return !iron.HasValue || (iron >= 0 && iron < 20000);
            });
            RuleFor(dto => dto.NutritionData.Potassium).Must(potassium =>
            {
                return !potassium.HasValue || (potassium >= 0 && potassium < 20000);
            });
            RuleFor(dto => dto.NutritionData.Magnesium).Must(magnesium =>
            {
                return !magnesium.HasValue || (magnesium >= 0 && magnesium < 20000);
            });

            RuleFor(dto => dto.PriceData.ProductSize).Must((dto, servingSize) =>
            {
                return servingSize != 0 || dto.PriceData.ProductSizeIsOneUnit;
            });
            RuleFor(dto => dto.PriceData.Price).Must(price =>
            {
                return !price.HasValue || (price > 0 && price < 20000);
            });
        }
    }
}
