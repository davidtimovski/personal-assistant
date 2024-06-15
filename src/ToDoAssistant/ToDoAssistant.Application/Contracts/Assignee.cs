using AutoMapper;
using Core.Application.Entities;
using Core.Application.Mappings;

namespace ToDoAssistant.Application.Contracts;

public class Assignee : IMapFrom<User>
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required string ImageUri { get; init; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<User, Assignee>();
    }
}
