using AutoMapper;
using CookingAssistant.Application.Entities;
using Core.Application.Mappings;

namespace CookingAssistant.Application.Contracts.Recipes.Models;

public class RecipeForSending : IMapFrom<Recipe>
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Recipe, RecipeForSending>();
    }
}
