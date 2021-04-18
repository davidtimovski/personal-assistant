using System;
using AutoMapper;
using PersonalAssistant.Application.Mappings;
using PersonalAssistant.Domain.Entities.Common;
using PersonalAssistant.Domain.Entities.CookingAssistant;

namespace PersonalAssistant.Application.Contracts.CookingAssistant.Recipes.Models
{
    public class RecipeShareDto : IMapFrom<RecipeShare>
    {
        public int UserId { get; set; }
        public string Email { get; set; }
        public string ImageUri { get; set; }
        public bool? IsAccepted { get; set; }
        public DateTime CreatedDate { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<RecipeShare, RecipeShareDto>()
                .ForMember(x => x.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(x => x.ImageUri, opt => opt.MapFrom(src => src.User.ImageUri));
        }
    }
}
