using AutoMapper;
using Chef.Application.Entities;
using Chef.Application.Mappings;
using Core.Application.Mappings;

namespace Chef.Application.Contracts.Recipes.Models;

public class RecipeWithShares : IMapFrom<Recipe>
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public RecipeSharingState SharingState { get; set; }
    public string OwnerEmail { get; set; } = null!;
    public string OwnerImageUri { get; set; } = null!;
    public RecipeShareDto? UserShare { get; set; }

    public List<RecipeShareDto> Shares { get; set; } = new();

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Recipe, RecipeWithShares>()
            .ForMember(x => x.SharingState, opt => opt.Ignore())
            .ForMember(x => x.OwnerEmail, opt => opt.MapFrom(src => src.User.Email))
            .ForMember(x => x.OwnerImageUri, opt => opt.MapFrom(src => src.User.ImageUri))
            .ForMember(x => x.UserShare, opt => opt.MapFrom<RecipeWithSharesUserShareResolver>());
    }
}
