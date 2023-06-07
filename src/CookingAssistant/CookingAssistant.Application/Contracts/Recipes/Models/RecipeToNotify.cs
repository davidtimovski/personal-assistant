using AutoMapper;
using CookingAssistant.Application.Entities;
using Core.Application.Mappings;

namespace CookingAssistant.Application.Contracts.Recipes.Models;

public class RecipeToNotify : IMapFrom<Recipe>
{
    public int UserId { get; set; }
    public string Name { get; set; } = null!;
    public string ImageUri { get; set; } = null!;

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Recipe, RecipeToNotify>();
    }
}
