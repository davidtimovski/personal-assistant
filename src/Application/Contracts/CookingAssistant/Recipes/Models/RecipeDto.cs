﻿using System;
using System.Collections.Generic;
using AutoMapper;
using PersonalAssistant.Application.Mappings;
using PersonalAssistant.Domain.Entities.CookingAssistant;

namespace PersonalAssistant.Application.Contracts.CookingAssistant.Recipes.Models
{
    public class RecipeDto : IMapFrom<Recipe>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<RecipeIngredientDto> Ingredients { get; set; }
        public string Instructions { get; set; }
        public string PrepDuration { get; set; }
        public string CookDuration { get; set; }
        public byte Servings { get; set; }
        public string ImageUri { get; set; }
        public string VideoUrl { get; set; }
        public DateTime LastOpenedDate { get; set; }
        public RecipeNutritionSummary NutritionSummary { get; set; }
        public RecipeCostSummary CostSummary { get; set; }
        public RecipeSharingState SharingState { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Recipe, RecipeDto>()
                .ForMember(x => x.Ingredients, opt => opt.MapFrom(src => src.RecipeIngredients))
                .ForMember(x => x.PrepDuration, opt => opt.MapFrom<DurationResolver, TimeSpan?>(src => src.PrepDuration))
                .ForMember(x => x.CookDuration, opt => opt.MapFrom<DurationResolver, TimeSpan?>(src => src.CookDuration))
                .ForMember(x => x.NutritionSummary, opt => opt.MapFrom<RecipeNutritionSummaryResolver>())
                .ForMember(x => x.CostSummary, opt => opt.MapFrom<RecipeCostSummaryResolver>())
                .ForMember(x => x.SharingState, opt => opt.MapFrom<RecipeSharingStateResolver>());
        }
    }
}
