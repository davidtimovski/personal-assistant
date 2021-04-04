using AutoMapper;
using PersonalAssistant.Application.Mappings;
using PersonalAssistant.Domain.Entities.CookingAssistant;
using PersonalAssistant.Domain.Entities.ToDoAssistant;

namespace PersonalAssistant.Application.Contracts.CookingAssistant.Ingredients.Models
{
    public class IngredientSuggestion : IMapFrom<Ingredient>, IMapFrom<ToDoTask>
    {
        public int Id { get; set; }
        public int? TaskId { get; set; }
        public string Name { get; set; }
        public string Label { get; set; }
        public string Group { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Ingredient, IngredientSuggestion>()
                .ForMember(x => x.Label, opt => opt.MapFrom(src => src.Name))
                .ForMember(x => x.Group, opt => opt.MapFrom(src => src.Task.List.Name));

            profile.CreateMap<ToDoTask, IngredientSuggestion>()
                .ForMember(x => x.TaskId, opt => opt.MapFrom(src => src.Id))
                .ForMember(x => x.Label, opt => opt.MapFrom(src => src.Name))
                .ForMember(x => x.Group, opt => opt.MapFrom(src => src.List.Name));
        }
    }
}
