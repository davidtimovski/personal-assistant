using Application.Domain.CookingAssistant;
using AutoMapper;
using CookingAssistant.Application.Mappings;
using Core.Application.Mappings;

namespace CookingAssistant.Application.Contracts.Recipes.Models;

public class SimpleRecipe : IMapFrom<Recipe>
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string ImageUri { get; set; }
    public short IngredientsMissing { get; set; }
    public DateTime LastOpenedDate { get; set; }
    public RecipeSharingState SharingState { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Recipe, SimpleRecipe>()
            .ForMember(x => x.LastOpenedDate, opt => opt.MapFrom<LastOpenedDateResolver>())
            .ForMember(x => x.SharingState, opt => opt.Ignore());
    }
}
