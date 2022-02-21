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
    public string Name { get; set; }
    public string Unit { get; set; }

    public List<IngredientSuggestion> Children { get; set; } = new List<IngredientSuggestion>();

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Ingredient, IngredientSuggestion>()
            .ForMember(x => x.Unit, opt => opt.Ignore())
            .ForMember(x => x.Children, opt => opt.Ignore());
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
