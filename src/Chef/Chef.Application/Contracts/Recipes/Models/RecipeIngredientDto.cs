using AutoMapper;
using Chef.Application.Entities;
using Chef.Application.Services;
using Core.Application.Mappings;

namespace Chef.Application.Contracts.Recipes.Models;

public class RecipeIngredientDto : IMapFrom<RecipeIngredient>
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? ParentName { get; set; }
    public float? Amount { get; set; }
    public float? AmountPerServing { get; set; }
    public string? Unit { get; set; }
    public bool HasNutritionData { get; set; }
    public bool HasPriceData { get; set; }
    public bool Missing { get; set; }
    public bool IsPublic { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<RecipeIngredient, RecipeIngredientDto>()
            .ForMember(x => x.Id, opt => opt.MapFrom(src => src.Ingredient.Id))
            .ForMember(x => x.Name, opt => opt.MapFrom(src => src.Ingredient.Name))
            .ForMember(x => x.ParentName, opt => opt.MapFrom(src => src.Ingredient.Parent == null ? null : src.Ingredient.Parent.Name))
            .ForMember(x => x.AmountPerServing, opt => opt.Ignore())
            .ForMember(x => x.HasNutritionData, opt => opt.MapFrom(src => NutritionDataHelper.Has(src.Ingredient)))
            .ForMember(x => x.HasPriceData, opt => opt.MapFrom(src => src.Ingredient.Price.HasValue))
            .ForMember(x => x.Missing, opt => opt.MapFrom(src => src.Ingredient.Task != null ? !src.Ingredient.Task.IsCompleted : false))
            .ForMember(x => x.IsPublic, opt => opt.MapFrom(src => src.Ingredient.UserId == 1));
    }
}
