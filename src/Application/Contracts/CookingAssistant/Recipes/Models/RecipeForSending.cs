using AutoMapper;
using Application.Mappings;
using Domain.Entities.CookingAssistant;

namespace Application.Contracts.CookingAssistant.Recipes.Models
{
    public class RecipeForSending : IMapFrom<Recipe>
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Recipe, RecipeForSending>();
        }
    }
}
