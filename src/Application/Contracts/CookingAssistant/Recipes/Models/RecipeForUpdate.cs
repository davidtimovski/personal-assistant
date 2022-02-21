using System;
using System.Collections.Generic;
using Application.Mappings;
using AutoMapper;
using Domain.Entities.CookingAssistant;

namespace Application.Contracts.CookingAssistant.Recipes.Models;

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
            .ForMember(x => x.PrepDuration, opt => opt.MapFrom<DurationResolver, TimeSpan?>(src => src.PrepDuration))
            .ForMember(x => x.CookDuration, opt => opt.MapFrom<DurationResolver, TimeSpan?>(src => src.CookDuration))
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
    public bool IsNew { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<RecipeIngredient, RecipeForUpdateIngredient>()
            .ForMember(x => x.Id, opt => opt.MapFrom(src => src.Ingredient.Id))
            .ForMember(x => x.Name, opt => opt.MapFrom(src => src.Ingredient.Name))
            .ForMember(x => x.Unit, opt => opt.MapFrom(src => src.Unit))
            .ForMember(x => x.IsNew, opt => opt.Ignore());
    }
}