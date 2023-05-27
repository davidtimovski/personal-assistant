using AutoMapper;
using Core.Application.Entities;
using Core.Application.Mappings;

namespace ToDoAssistant.Application.Contracts;

public class Assignee : IMapFrom<User>
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string ImageUri { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<User, Assignee>();
    }
}
