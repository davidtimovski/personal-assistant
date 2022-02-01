using System.Collections.Generic;
using AutoMapper;
using Application.Mappings;
using Domain.Entities.CookingAssistant;

namespace Application.Contracts.CookingAssistant.Recipes.Models
{
    public class RecipeForReview : IMapFrom<Recipe>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUri { get; set; }
        public IEnumerable<IngredientForReview> Ingredients { get; set; }
        public IEnumerable<IngredientReviewSuggestion> IngredientSuggestions { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Recipe, RecipeForReview>()
                .ForMember(x => x.Ingredients, opt => opt.MapFrom(src => src.RecipeIngredients))
                .ForMember(x => x.IngredientSuggestions, opt => opt.Ignore());
        }
    }

    public class IngredientForReview : IMapFrom<RecipeIngredient>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool HasNutritionData { get; set; }
        public bool HasPriceData { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<RecipeIngredient, IngredientForReview>()
                .ForMember(x => x.Id, opt => opt.MapFrom(src => src.IngredientId))
                .ForMember(x => x.Name, opt => opt.MapFrom(src => src.Ingredient.Name))
                .ForMember(x => x.HasNutritionData, opt => opt.MapFrom<RecipeIngredientHasNutritionDataResolver>())
                .ForMember(x => x.HasPriceData, opt => opt.MapFrom<RecipeIngredientHasPriceDataResolver>());
        }
    }

    public class IngredientReviewSuggestion : IMapFrom<Ingredient>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Label { get; set; }
        public bool HasNutritionData { get; set; }
        public bool HasPriceData { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Ingredient, IngredientReviewSuggestion>()
                .ForMember(x => x.Label, opt => opt.MapFrom(src => src.Name))
                .ForMember(x => x.HasNutritionData, opt => opt.MapFrom<IngredientHasNutritionDataResolver>())
                .ForMember(x => x.HasPriceData, opt => opt.MapFrom<IngredientHasPriceDataResolver>());
        }
    }
}
