using AutoMapper;
using CookingAssistant.Application.Entities;
using Core.Application.Mappings;

namespace CookingAssistant.Application.Contracts.Ingredients.Models;

public class IngredientNutritionData : IMapFrom<Ingredient>
{
    public bool IsSet { get; set; }
    public float ServingSize { get; set; }
    public bool ServingSizeIsOneUnit { get; set; }
    public float? Calories { get; set; }
    public float? Fat { get; set; }
    public float? SaturatedFat { get; set; }
    public float? Carbohydrate { get; set; }
    public float? Sugars { get; set; }
    public float? AddedSugars { get; set; }
    public float? Fiber { get; set; }
    public float? Protein { get; set; }
    public float? Sodium { get; set; }
    public float? Cholesterol { get; set; }
    public float? VitaminA { get; set; }
    public float? VitaminC { get; set; }
    public float? VitaminD { get; set; }
    public float? Calcium { get; set; }
    public float? Iron { get; set; }
    public float? Potassium { get; set; }
    public float? Magnesium { get; set; }

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
