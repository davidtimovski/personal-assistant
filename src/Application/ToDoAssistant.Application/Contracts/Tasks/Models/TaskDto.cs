using AutoMapper;
using Core.Application.Mappings;
using ToDoAssistant.Application.Entities;

namespace ToDoAssistant.Application.Contracts.Tasks.Models;

public class TaskDto : IMapFrom<ToDoTask>
{
    public int Id { get; set; }
    public int ListId { get; set; }
    public string Name { get; set; }
    public string Url { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsOneTime { get; set; }
    public bool IsHighPriority { get; set; }
    public bool IsPrivate { get; set; }
    public Assignee AssignedUser { get; set; }
    public short Order { get; set; }
    public DateTime ModifiedDate { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<ToDoTask, TaskDto>()
            .ForMember(x => x.IsPrivate, opt => opt.MapFrom(src => src.PrivateToUserId.HasValue))
            .ForMember(x => x.AssignedUser, opt => opt.MapFrom(src => src.AssignedToUser));
    }
}
