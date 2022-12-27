using Application.Domain.ToDoAssistant;
using AutoMapper;
using Core.Application.Mappings;

namespace CookingAssistant.Application.Contracts.Ingredients.Models;

public class TaskSuggestion : IMapFrom<ToDoTask>
{
    public int Id { get; set; }
    public string Label { get; set; }
    public string Group { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<ToDoTask, TaskSuggestion>()
            .ForMember(x => x.Label, opt => opt.MapFrom(src => src.Name))
            .ForMember(x => x.Group, opt => opt.MapFrom(src => src.List.Name));
    }
}
