using System.Collections.Generic;
using Application.Mappings;
using AutoMapper;
using Domain.Entities.CookingAssistant;

namespace Application.Contracts.CookingAssistant.Ingredients.Models;

public class ViewIngredient : IMapFrom<Ingredient>
{
    public int Id { get; set; }
    public int? TaskId { get; set; }
    public string TaskName { get; set; }
    public string TaskList { get; set; }
    public string BrandName { get; set; }
    public string Name { get; set; }
    public IngredientNutritionData NutritionData { get; set; } = new IngredientNutritionData();
    public IngredientPriceData PriceData { get; set; } = new IngredientPriceData();
    public List<string> Recipes { get; set; } = new List<string>();

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Ingredient, ViewIngredient>()
            .ForMember(x => x.TaskName, opt => opt.MapFrom(src => src.TaskId.HasValue ? src.Task.Name : null))
            .ForMember(x => x.TaskList, opt => opt.MapFrom(src => src.TaskId.HasValue ? src.Task.List.Name : null))
            .ForMember(x => x.BrandName, opt => opt.MapFrom(src => src.Brand != null ? src.Brand.Name : null))
            .ForMember(x => x.NutritionData, opt => opt.MapFrom<NutritionDataResolver>())
            .ForMember(x => x.PriceData, opt => opt.MapFrom<PriceDataResolver>())
            .ForMember(x => x.Recipes, opt => opt.MapFrom<RecipeNameResolver, List<Recipe>>(src => src.Recipes));
    }
}