using AutoMapper;
using Core.Application.Mappings;
using ToDoAssistant.Application.Entities;

namespace ToDoAssistant.Application.Contracts.Lists.Models;

public class ToDoListOption : IMapFrom<ToDoList>
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public bool IsShared { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<ToDoList, ToDoListOption>()
            .ForMember(x => x.IsShared, opt => opt.MapFrom(x => x.Shares.Any()));
    }
}
