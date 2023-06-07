using AutoMapper;
using CookingAssistant.Application.Entities;
using Core.Application.Mappings;

namespace CookingAssistant.Application.Contracts.Recipes.Models;

public class ShareRecipeRequest : IMapFrom<RecipeShare>
{
    public int RecipeId { get; set; }
    public string RecipeName { get; set; } = null!;
    public string RecipeOwnerName { get; set; } = null!;
    public bool? IsAccepted { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<RecipeShare, ShareRecipeRequest>()
            .ForMember(x => x.RecipeName, opt => opt.MapFrom(src => src.Recipe.Name))
            .ForMember(x => x.RecipeOwnerName, opt => opt.MapFrom(src => src.User.Name));
    }
}
