using AutoMapper;
using PersonalAssistant.Application.Mappings;
using PersonalAssistant.Domain.Entities.CookingAssistant;

namespace PersonalAssistant.Application.Contracts.CookingAssistant.Recipes.Models
{
    public class RecipeForUpdateIngredient : IMapFrom<RecipeIngredient>
    {
        public int Id { get; set; }
        public int? TaskId { get; set; }
        public string TaskList { get; set; }
        public string Name { get; set; }
        public float? Amount { get; set; }
        public string Unit { get; set; }
        public bool Existing { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<RecipeIngredient, RecipeForUpdateIngredient>()
                .ForMember(x => x.Id, opt => opt.MapFrom(src => src.Ingredient.Id))
                .ForMember(x => x.TaskId, opt => opt.MapFrom(src => src.Ingredient.TaskId))
                .ForMember(x => x.TaskList, opt => opt.MapFrom(src => src.Ingredient.Task != null ? src.Ingredient.Task.List.Name : null))
                .ForMember(x => x.Name, opt => opt.MapFrom(src => src.Ingredient.Name))
                .ForMember(x => x.Unit, opt => opt.MapFrom(src => src.Unit))
                .ForMember(x => x.Existing, opt => opt.MapFrom(src => true));
        }
    }
}
