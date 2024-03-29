﻿using AutoMapper;
using Chef.Application.Entities;
using Chef.Application.Services;
using Core.Application.Mappings;

namespace Chef.Application.Contracts.Recipes.Models;

public class RecipeForReview : IMapFrom<Recipe>
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string ImageUri { get; set; } = null!;
    public IReadOnlyList<IngredientForReview> Ingredients { get; set; } = new List<IngredientForReview>();

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Recipe, RecipeForReview>()
            .ForMember(x => x.Ingredients, opt => opt.MapFrom(src => src.RecipeIngredients.Select(ri => ri.Ingredient)));
    }
}

public class IngredientForReview : IMapFrom<Ingredient>
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public bool HasNutritionData { get; set; }
    public bool HasPriceData { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Ingredient, IngredientForReview>()
            .ForMember(x => x.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(x => x.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(x => x.HasNutritionData, opt => opt.MapFrom(src => NutritionDataHelper.Has(src)))
            .ForMember(x => x.HasPriceData, opt => opt.MapFrom(src => src.Price.HasValue));
    }
}
