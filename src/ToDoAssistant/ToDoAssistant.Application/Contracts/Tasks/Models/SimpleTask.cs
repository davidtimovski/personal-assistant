using AutoMapper;
using Core.Application.Mappings;
using ToDoAssistant.Application.Entities;

namespace ToDoAssistant.Application.Contracts.Tasks.Models;

public class SimpleTask : IMapFrom<ToDoTask>
{
    public int Id { get; set; }
    public int ListId { get; set; }
    public string Name { get; set; } = null!;
    public int? PrivateToUserId { get; set; }
    public int? AssignedToUserId { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<ToDoTask, SimpleTask>();
    }
}
