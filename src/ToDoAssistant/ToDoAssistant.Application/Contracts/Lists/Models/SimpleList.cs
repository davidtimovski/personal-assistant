using AutoMapper;
using Core.Application.Mappings;
using ToDoAssistant.Application.Entities;

namespace ToDoAssistant.Application.Contracts.Lists.Models;

public class SimpleList : IMapFrom<ToDoList>
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Icon { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<ToDoList, SimpleList>();
    }
}
