using Application.Mappings;
using AutoMapper;
using Domain.Entities.CookingAssistant;

namespace Application.Contracts.CookingAssistant.Ingredients.Models;

public class IngredientDto : IMapFrom<Ingredient>
{
    public int Id { get; set; }
    public int? TaskId { get; set; }
    public string Name { get; set; }
    public bool HasNutritionData { get; set; }
    public bool HasPriceData { get; set; }
    public bool Unused { get; set; }
    public bool IsPublic { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Ingredient, IngredientDto>()
            .ForMember(x => x.HasNutritionData, opt => opt.MapFrom<IngredientHasNutritionDataResolver>())
            .ForMember(x => x.HasPriceData, opt => opt.MapFrom<IngredientHasPriceDataResolver>())
            .ForMember(x => x.Unused, opt => opt.MapFrom(src => src.RecipeCount == 0))
            .ForMember(x => x.IsPublic, opt => opt.MapFrom(src => src.UserId == 1));
    }
}