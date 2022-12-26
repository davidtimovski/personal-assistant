using Application.Domain.CookingAssistant;
using AutoMapper;
using Core.Application.Mappings;

namespace CookingAssistant.Application.Contracts.Recipes.Models;

public class RecipeToNotify : IMapFrom<Recipe>
{
    public int UserId { get; set; }
    public string Name { get; set; }
    public string ImageUri { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Recipe, RecipeToNotify>();
    }
}
