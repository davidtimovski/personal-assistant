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
    public int? TaskId { get; set; }
    public string Name { get; set; }
    public string Unit { get; set; }
    public bool Selected { get; set; }

    public IEnumerable<IngredientSuggestion> Children { get; set; } = new List<IngredientSuggestion>();

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Ingredient, IngredientSuggestion>()
            .ForMember(x => x.Unit, opt => opt.Ignore())
            .ForMember(x => x.Selected, opt => opt.Ignore())
            .ForMember(x => x.Children, opt => opt.Ignore());
    }
}

public class IngredientCategoryDto : IMapFrom<IngredientCategory>
{
    public int Id { get; set; }
    public int? ParentId { get; set; }
    public string Name { get; set; }
    
    public IEnumerable<IngredientSuggestion> Ingredients { get; set; } = new List<IngredientSuggestion>();
    public IEnumerable<IngredientCategoryDto> Subcategories { get; set; } = new List<IngredientCategoryDto>();

    public void Mapping(Profile profile)
    {
        profile.CreateMap<IngredientCategory, IngredientCategoryDto>();
    }
}

public class PublicIngredientSuggestions
{
    public IEnumerable<IngredientSuggestion> Uncategorized { get; set; } = new List<IngredientSuggestion>();
    public IEnumerable<IngredientCategoryDto> Categories { get; set; } = new List<IngredientCategoryDto>();
}

public class IngredientSuggestionsDto
{
    public IEnumerable<IngredientSuggestion> UserIngredients { get; set; }
    public PublicIngredientSuggestions PublicIngredients { get; set; } = new PublicIngredientSuggestions();
}
