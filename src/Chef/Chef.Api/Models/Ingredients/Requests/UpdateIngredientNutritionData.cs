using System.ComponentModel.DataAnnotations;

namespace Chef.Api.Models.Ingredients.Requests;

public record UpdateIngredientNutritionData([Required] bool IsSet, [Required] float ServingSize, [Required] bool ServingSizeIsOneUnit, [Required] float? Calories, [Required] float? Fat, [Required] float? SaturatedFat, [Required] float? Carbohydrate, [Required] float? Sugars, [Required] float? AddedSugars, [Required] float? Fiber, [Required] float? Protein, [Required] float? Sodium, [Required] float? Cholesterol, [Required] float? VitaminA, [Required] float? VitaminC, [Required] float? VitaminD, [Required] float? Calcium, [Required] float? Iron, [Required] float? Potassium, [Required] float? Magnesium);
