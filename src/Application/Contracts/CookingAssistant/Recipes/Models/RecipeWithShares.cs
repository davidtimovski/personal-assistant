﻿using System.Collections.Generic;
using AutoMapper;
using PersonalAssistant.Application.Mappings;
using PersonalAssistant.Domain.Entities.CookingAssistant;

namespace PersonalAssistant.Application.Contracts.CookingAssistant.Recipes.Models
{
    public class RecipeWithShares : IMapFrom<Recipe>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public RecipeSharingState SharingState { get; set; }
        public string OwnerEmail { get; set; }
        public string OwnerImageUri { get; set; }
        public RecipeShareDto UserShare { get; set; }

        public List<RecipeShareDto> Shares { get; set; } = new List<RecipeShareDto>();

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Recipe, RecipeWithShares>()
                .ForMember(x => x.SharingState, opt => opt.MapFrom<RecipeSharingStateResolver>())
                .ForMember(x => x.OwnerEmail, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(x => x.OwnerImageUri, opt => opt.MapFrom(src => src.User.ImageUri))
                .ForMember(x => x.UserShare, opt => opt.MapFrom<RecipeWithSharesUserShareResolver>());
        }
    }
}
