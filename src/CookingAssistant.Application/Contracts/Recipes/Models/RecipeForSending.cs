using Application.Mappings;
using AutoMapper;
using Domain.CookingAssistant;

namespace CookingAssistant.Application.Contracts.Recipes.Models;

public class RecipeForSending : IMapFrom<Recipe>
{
    public int Id { get; set; }
    public string Name { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Recipe, RecipeForSending>();
    }
}