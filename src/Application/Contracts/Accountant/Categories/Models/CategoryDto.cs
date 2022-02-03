﻿using System;
using AutoMapper;
using Application.Mappings;
using Domain.Entities.Accountant;

namespace Application.Contracts.Accountant.Categories.Models;

public class CategoryDto : IMapFrom<Category>
{
    public int Id { get; set; }
    public int? ParentId { get; set; }
    public string Name { get; set; }
    public CategoryType Type { get; set; }
    public bool GenerateUpcomingExpense { get; set; }
    public bool IsTax { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Category, CategoryDto>();
    }
}