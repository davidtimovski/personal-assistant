using Application.Mappings;
using AutoMapper;
using Domain.CookingAssistant;

namespace CookingAssistant.Application.Contracts.Ingredients.Models;

public class EditIngredient : IMapFrom<Ingredient>
{
    public int Id { get; set; }
    public int? TaskId { get; set; }
    public string TaskName { get; set; }
    public string TaskList { get; set; }
    public string Name { get; set; }
    public IngredientNutritionData NutritionData { get; set; } = new IngredientNutritionData();
    public IngredientPriceData PriceData { get; set; } = new IngredientPriceData();
    public List<SimpleRecipe> Recipes { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Ingredient, EditIngredient>()
            .ForMember(x => x.TaskName, opt => opt.MapFrom(src => src.TaskId.HasValue ? src.Task.Name : null))
            .ForMember(x => x.TaskList, opt => opt.MapFrom(src => src.TaskId.HasValue ? src.Task.List.Name : null))
            .ForMember(x => x.NutritionData, opt => opt.MapFrom(src => src))
            .ForMember(x => x.PriceData, opt => opt.MapFrom(src => src));
    }
}
