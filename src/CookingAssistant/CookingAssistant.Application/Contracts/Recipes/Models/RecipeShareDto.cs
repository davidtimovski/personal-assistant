using AutoMapper;
using CookingAssistant.Application.Entities;
using Core.Application.Mappings;

namespace CookingAssistant.Application.Contracts.Recipes.Models;

public class RecipeShareDto : IMapFrom<RecipeShare>
{
    public int UserId { get; set; }
    public string Email { get; set; } = null!;
    public string ImageUri { get; set; } = null!;
    public bool? IsAccepted { get; set; }
    public DateTime CreatedDate { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<RecipeShare, RecipeShareDto>()
            .ForMember(x => x.Email, opt => opt.MapFrom(src => src.User.Email))
            .ForMember(x => x.ImageUri, opt => opt.MapFrom(src => src.User.ImageUri));
    }
}
