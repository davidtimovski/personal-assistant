using Application.Mappings;
using AutoMapper;
using Domain.Accountant;

namespace Accountant.Application.Contracts.Categories.Models;

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