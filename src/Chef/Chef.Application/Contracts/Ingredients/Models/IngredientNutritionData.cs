using AutoMapper;
using Chef.Application.Entities;
using Core.Application.Mappings;

namespace Chef.Application.Contracts.Ingredients.Models;

public class IngredientNutritionData : IMapFrom<Ingredient>
{
    public bool IsSet { get; init; }
    public float ServingSize { get; init; }
    public bool ServingSizeIsOneUnit { get; init; }
    public float? Calories { get; init; }
    public float? Fat { get; init; }
    public float? SaturatedFat { get; init; }
    public float? Carbohydrate { get; init; }
    public float? Sugars { get; init; }
    public float? AddedSugars { get; init; }
    public float? Fiber { get; init; }
    public float? Protein { get; init; }
    public float? Sodium { get; init; }
    public float? Cholesterol { get; init; }
    public float? VitaminA { get; init; }
    public float? VitaminC { get; init; }
    public float? VitaminD { get; init; }
    public float? Calcium { get; init; }
    public float? Iron { get; init; }
    public float? Potassium { get; init; }
    public float? Magnesium { get; init; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Ingredient, IngredientNutritionData>()
            .ForMember(x => x.IsSet, opt => opt.MapFrom(src => src.Calories.HasValue
                || src.Fat.HasValue
                || src.SaturatedFat.HasValue
                || src.Carbohydrate.HasValue
                || src.Sugars.HasValue
                || src.AddedSugars.HasValue
                || src.Fiber.HasValue
                || src.Protein.HasValue
                || src.Sodium.HasValue
                || src.Cholesterol.HasValue
                || src.VitaminA.HasValue
                || src.VitaminC.HasValue
                || src.VitaminD.HasValue
                || src.Calcium.HasValue
                || src.Iron.HasValue
                || src.Potassium.HasValue
                || src.Magnesium.HasValue));
    }
}
