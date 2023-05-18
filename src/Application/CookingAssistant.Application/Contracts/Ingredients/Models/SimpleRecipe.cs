﻿using AutoMapper;
using CookingAssistant.Application.Entities;
using Core.Application.Mappings;

namespace CookingAssistant.Application.Contracts.Ingredients.Models;

public class SimpleRecipe : IMapFrom<Recipe>
{
    public int Id { get; set; }
    public string Name { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Recipe, SimpleRecipe>();
    }
}
