using AutoMapper;
using CookingAssistant.Application.Entities;
using CookingAssistant.Application.Services;
using Core.Application.Mappings;

namespace CookingAssistant.Application.Contracts.Recipes.Models;

public class RecipeForUpdate : IMapFrom<Recipe>
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public List<RecipeForUpdateIngredient> Ingredients { get; set; }
    public string Instructions { get; set; }
    public string PrepDuration { get; set; }
    public string CookDuration { get; set; }
    public byte Servings { get; set; }
    public string ImageUri { get; set; }
    public string VideoUrl { get; set; }
    public RecipeSharingState SharingState { get; set; }
    public bool UserIsOwner { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Recipe, RecipeForUpdate>()
            .ForMember(x => x.Ingredients, opt => opt.MapFrom(src => src.RecipeIngredients))
            .ForMember(x => x.PrepDuration, opt => opt.MapFrom(src => src.PrepDuration.HasValue ? src.PrepDuration.Value.ToString(@"hh\:mm") : string.Empty))
            .ForMember(x => x.CookDuration, opt => opt.MapFrom(src => src.CookDuration.HasValue ? src.CookDuration.Value.ToString(@"hh\:mm") : string.Empty))
            .ForMember(x => x.SharingState, opt => opt.Ignore())
            .ForMember(x => x.UserIsOwner, opt => opt.Ignore());
    }
}

public class RecipeForUpdateIngredient : IMapFrom<RecipeIngredient>
{
    public int Id { get; set; }
    public string Name { get; set; }
    public float? Amount { get; set; }
    public string Unit { get; set; }
    public bool HasNutritionData { get; set; }
    public bool HasPriceData { get; set; }
    public bool IsPublic { get; set; }
    public bool IsNew { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<RecipeIngredient, RecipeForUpdateIngredient>()
            .ForMember(x => x.Id, opt => opt.MapFrom(src => src.Ingredient.Id))
            .ForMember(x => x.Name, opt => opt.MapFrom(src => src.Ingredient.Name))
            .ForMember(x => x.Unit, opt => opt.MapFrom(src => src.Unit))
            .ForMember(x => x.HasNutritionData, opt => opt.MapFrom(src => NutritionDataHelper.Has(src.Ingredient)))
            .ForMember(x => x.HasPriceData, opt => opt.MapFrom(src => src.Ingredient.Price.HasValue))
            .ForMember(x => x.IsPublic, opt => opt.MapFrom(src => src.Ingredient.UserId == 1))
            .ForMember(x => x.IsNew, opt => opt.Ignore());
    }
}
