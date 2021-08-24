using System;
using AutoMapper;
using PersonalAssistant.Application.Mappings;
using PersonalAssistant.Domain.Entities.CookingAssistant;

namespace PersonalAssistant.Application.Contracts.CookingAssistant.Recipes.Models
{
    public class RecipeToNotify : IMapFrom<Recipe>
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string ImageUri { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Recipe, RecipeToNotify>();
        }
    }
}
