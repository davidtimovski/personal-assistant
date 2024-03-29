﻿using AutoMapper;
using Chef.Application.Entities;
using Core.Application.Mappings;

namespace Chef.Application.Contracts.Recipes.Models;

public class RecipeDto : IMapFrom<Recipe>
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public List<RecipeIngredientDto> Ingredients { get; set; } = null!;
    public string? Instructions { get; set; }
    public string PrepDuration { get; set; } = null!;
    public string CookDuration { get; set; } = null!;
    public byte Servings { get; set; }
    public string ImageUri { get; set; } = null!;
    public string? VideoUrl { get; set; }
    public DateTime LastOpenedDate { get; set; }
    public RecipeNutritionSummary NutritionSummary { get; set; }
    public RecipeCostSummary CostSummary { get; set; }
    public RecipeSharingState SharingState { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Recipe, RecipeDto>()
            .ForMember(x => x.Ingredients, opt => opt.MapFrom(src => src.RecipeIngredients))
            .ForMember(x => x.PrepDuration, opt => opt.MapFrom(src => src.PrepDuration.HasValue ? src.PrepDuration.Value.ToString(@"hh\:mm") : string.Empty))
            .ForMember(x => x.CookDuration, opt => opt.MapFrom(src => src.CookDuration.HasValue ? src.CookDuration.Value.ToString(@"hh\:mm") : string.Empty))
            .ForMember(x => x.NutritionSummary, opt => opt.Ignore())
            .ForMember(x => x.CostSummary, opt => opt.Ignore())
            .ForMember(x => x.SharingState, opt => opt.Ignore());
    }
}
