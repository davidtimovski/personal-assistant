using AutoMapper;
using CookingAssistant.Application.Entities;
using Core.Application.Mappings;

namespace CookingAssistant.Application.Contracts.Recipes.Models;

public class SendRequestDto : IMapFrom<SendRequest>
{
    public int RecipeId { get; set; }
    public string RecipeName { get; set; } = null!;
    public string RecipeSenderName { get; set; } = null!;
    public bool IsDeclined { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<SendRequest, SendRequestDto>()
            .ForMember(x => x.RecipeName, opt => opt.MapFrom(src => src.Recipe.Name))
            .ForMember(x => x.RecipeSenderName, opt => opt.MapFrom(src => src.User.Name));
    }
}
