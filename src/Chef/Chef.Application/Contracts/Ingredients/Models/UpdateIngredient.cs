using FluentValidation;

namespace Chef.Application.Contracts.Ingredients.Models;

public class UpdateIngredient
{
    public required int UserId { get; init; }
    public required int Id { get; init; }
    public required int? TaskId { get; init; }
    public required string Name { get; init; } = null!;
    public required UpdateIngredientNutritionData NutritionData { get; init; }
    public required UpdateIngredientPriceData PriceData { get; init; }
}

public class UpdateIngredientValidator : AbstractValidator<UpdateIngredient>
{
    public UpdateIngredientValidator(IIngredientService ingredientService)
    {
        RuleFor(dto => dto.UserId)
            .NotEmpty().WithMessage("Unauthorized")
            .Must((dto, userId) => ingredientService.Exists(dto.Id, userId)).WithMessage("Unauthorized")
            .Must((dto, userId) => !ingredientService.Exists(dto.Id, dto.Name, userId)).WithMessage("AlreadyExists");

        // TODO
        //RuleFor(dto => dto.TaskId)
        //    .Must((dto, taskId) => !taskId.HasValue || taskService.Exists(taskId.Value, dto.UserId)).WithMessage("IsLinkedToNonExistentTask");

        RuleFor(dto => dto.Name).Must((dto, name) => dto.TaskId.HasValue ? true : !string.IsNullOrWhiteSpace(name)).WithMessage("Ingredients.UpdateIngredient.NameIsRequired")
            .MaximumLength(50).WithMessage("Ingredients.UpdateIngredient.NameMaxLength");

        RuleFor(dto => dto.NutritionData.ServingSize).Must((dto, servingSize) => servingSize != 0 || dto.NutritionData.ServingSizeIsOneUnit);
        RuleFor(dto => dto.NutritionData.Calories).Must(calories => !calories.HasValue || calories >= 0 && calories < 1000);
        RuleFor(dto => dto.NutritionData.Fat).Must(fat => !fat.HasValue || fat >= 0 && fat < 1000);
        RuleFor(dto => dto.NutritionData.SaturatedFat).Must(saturatedFat => !saturatedFat.HasValue || saturatedFat >= 0 && saturatedFat < 1000);
        RuleFor(dto => dto.NutritionData.Carbohydrate).Must(carbohydrate => !carbohydrate.HasValue || carbohydrate >= 0 && carbohydrate < 1000);
        RuleFor(dto => dto.NutritionData.Sugars).Must(sugars => !sugars.HasValue || sugars >= 0 && sugars < 1000);
        RuleFor(dto => dto.NutritionData.AddedSugars).Must(addedSugars => !addedSugars.HasValue || addedSugars >= 0 && addedSugars < 1000);
        RuleFor(dto => dto.NutritionData.Fiber).Must(fiber => !fiber.HasValue || fiber >= 0 && fiber < 1000);
        RuleFor(dto => dto.NutritionData.Protein).Must(protein => !protein.HasValue || protein >= 0 && protein < 1000);
        RuleFor(dto => dto.NutritionData.Sodium).Must(sodium => !sodium.HasValue || sodium >= 0 && sodium < 20000);
        RuleFor(dto => dto.NutritionData.Cholesterol).Must(cholesterol => !cholesterol.HasValue || cholesterol >= 0 && cholesterol < 20000);
        RuleFor(dto => dto.NutritionData.VitaminA).Must(vitaminA => !vitaminA.HasValue || vitaminA >= 0 && vitaminA < 20000);
        RuleFor(dto => dto.NutritionData.VitaminC).Must(vitaminC => !vitaminC.HasValue || vitaminC >= 0 && vitaminC < 20000);
        RuleFor(dto => dto.NutritionData.VitaminD).Must(vitaminD => !vitaminD.HasValue || vitaminD >= 0 && vitaminD < 20000);
        RuleFor(dto => dto.NutritionData.Calcium).Must(calcium => !calcium.HasValue || calcium >= 0 && calcium < 20000);
        RuleFor(dto => dto.NutritionData.Iron).Must(iron => !iron.HasValue || iron >= 0 && iron < 20000);
        RuleFor(dto => dto.NutritionData.Potassium).Must(potassium => !potassium.HasValue || potassium >= 0 && potassium < 20000);
        RuleFor(dto => dto.NutritionData.Magnesium).Must(magnesium => !magnesium.HasValue || magnesium >= 0 && magnesium < 20000);

        RuleFor(dto => dto.PriceData.ProductSize).Must((dto, servingSize) => servingSize != 0 || dto.PriceData.ProductSizeIsOneUnit);
        RuleFor(dto => dto.PriceData.Price).Must(price => !price.HasValue || price > 0 && price < 20000);
    }
}