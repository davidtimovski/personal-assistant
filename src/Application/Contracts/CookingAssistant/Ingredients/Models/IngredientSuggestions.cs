using System.Collections.Generic;
using Application.Mappings;
using AutoMapper;
using Domain.Entities.CookingAssistant;

namespace Application.Contracts.CookingAssistant.Ingredients.Models;

public class IngredientSuggestion : IMapFrom<Ingredient>
{
    public int Id { get; set; }
    public int? ParentId { get; set; }
    public int? CategoryId { get; set; }
    public string BrandName { get; set; }
    public string Name { get; set; }
    public bool IsProduct { get; set; }
    public string Unit { get; set; }
    public string UnitImperial { get; set; }
    public bool HasNutritionData { get; set; }
    public bool HasPriceData { get; set; }
    public bool IsPublic { get; set; }

    public List<IngredientSuggestion> Children { get; set; } = new List<IngredientSuggestion>();

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Ingredient, IngredientSuggestion>()
            .ForMember(x => x.BrandName, opt => opt.MapFrom(src => src.Brand != null ? src.Brand.Name : null))
            .ForMember(x => x.Unit, opt => opt.Ignore())
            .ForMember(x => x.UnitImperial, opt => opt.Ignore())
            .ForMember(x => x.Children, opt => opt.Ignore())
            .ForMember(x => x.HasNutritionData, opt => opt.MapFrom<IngredientHasNutritionDataResolver>())
            .ForMember(x => x.HasPriceData, opt => opt.MapFrom<IngredientHasPriceDataResolver>())
            .ForMember(x => x.IsPublic, opt => opt.MapFrom(src => src.UserId == 1));
    }
}

public class IngredientCategoryDto : IMapFrom<IngredientCategory>
{
    public int Id { get; set; }
    public int? ParentId { get; set; }
    public string Name { get; set; }
    
    public List<IngredientSuggestion> Ingredients { get; set; }
    public List<IngredientCategoryDto> Subcategories { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<IngredientCategory, IngredientCategoryDto>()
            .ForMember(x => x.Ingredients, opt => opt.Ignore())
            .ForMember(x => x.Subcategories, opt => opt.Ignore());
    }
}

public class PublicIngredientSuggestions
{
    public List<IngredientSuggestion> Uncategorized { get; set; } = new List<IngredientSuggestion>();
    public List<IngredientCategoryDto> Categories { get; set; } = new List<IngredientCategoryDto>();
}
