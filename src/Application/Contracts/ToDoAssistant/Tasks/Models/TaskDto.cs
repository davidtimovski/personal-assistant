using AutoMapper;
using Application.Mappings;
using Domain.Entities.ToDoAssistant;
using System;

namespace Application.Contracts.ToDoAssistant.Tasks.Models;

public class TaskDto : IMapFrom<ToDoTask>
{
    public int Id { get; set; }
    public int ListId { get; set; }
    public string Name { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsOneTime { get; set; }
    public bool IsHighPriority { get; set; }
    public bool IsPrivate { get; set; }
    public AssignedUser AssignedUser { get; set; }
    public short Order { get; set; }
    public DateTime ModifiedDate { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<ToDoTask, TaskDto>()
            .ForMember(x => x.IsPrivate, opt => opt.MapFrom(src => src.PrivateToUserId.HasValue))
            .ForMember(x => x.AssignedUser, opt => opt.MapFrom<TaskAssignedUserResolver>());
    }
}
