using Application.Mappings;
using AutoMapper;
using Domain.ToDoAssistant;
using ToDoAssistant.Application.Mappings;

namespace ToDoAssistant.Application.Contracts.Tasks.Models;

public class TaskForUpdate : IMapFrom<ToDoTask>
{
    public int Id { get; set; }
    public int ListId { get; set; }
    public string Name { get; set; }
    public string Url { get; set; }
    public bool IsOneTime { get; set; }
    public bool IsPrivate { get; set; }
    public bool IsHighPriority { get; set; }
    public int? AssignedToUserId { get; set; }
    public bool IsInSharedList { get; set; }
    public List<string> Recipes { get; set; } = new List<string>();

    public void Mapping(Profile profile)
    {
        profile.CreateMap<ToDoTask, TaskForUpdate>()
            .ForMember(x => x.IsPrivate, opt => opt.MapFrom<IsPrivateResolver>())
            .ForMember(x => x.IsInSharedList, opt => opt.Ignore())
            .ForMember(x => x.Recipes, opt => opt.Ignore());
    }
}
