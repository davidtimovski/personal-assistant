using System.Collections.Generic;
using Application.Mappings;
using Application.Services.CookingAssistant;
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
    public bool IsProduct { get; set; }
    public IngredientNutritionData NutritionData { get; set; }
    public IngredientPriceData PriceData { get; set; }
    public List<SimpleRecipe> Recipes { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Ingredient, ViewIngredient>()
            .ForMember(x => x.TaskName, opt => opt.MapFrom(src => src.TaskId.HasValue ? src.Task.Name : null))
            .ForMember(x => x.TaskList, opt => opt.MapFrom(src => src.TaskId.HasValue ? src.Task.List.Name : null))
            .ForMember(x => x.BrandName, opt => opt.MapFrom(src => src.Brand != null ? src.Brand.Name : null))
            .ForMember(x => x.NutritionData, opt => opt.MapFrom(src => src))
            .ForMember(x => x.PriceData, opt => opt.MapFrom(src => src));
    }
}
