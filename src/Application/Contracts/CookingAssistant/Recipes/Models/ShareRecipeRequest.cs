using AutoMapper;
using Application.Mappings;
using Domain.Entities.CookingAssistant;

namespace Application.Contracts.CookingAssistant.Recipes.Models
{
    public class ShareRecipeRequest : IMapFrom<RecipeShare>
    {
        public int RecipeId { get; set; }
        public string RecipeName { get; set; }
        public string RecipeOwnerName { get; set; }
        public bool? IsAccepted { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<RecipeShare, ShareRecipeRequest>()
                .ForMember(x => x.RecipeName, opt => opt.MapFrom(src => src.Recipe.Name))
                .ForMember(x => x.RecipeOwnerName, opt => opt.MapFrom(src => src.User.Name));
        }
    }
}
