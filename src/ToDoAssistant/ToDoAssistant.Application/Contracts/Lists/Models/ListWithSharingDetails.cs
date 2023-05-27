using AutoMapper;
using Core.Application.Mappings;
using ToDoAssistant.Application.Contracts.Tasks.Models;
using ToDoAssistant.Application.Entities;
using ToDoAssistant.Application.Mappings;

namespace ToDoAssistant.Application.Contracts.Lists.Models;

public class ListWithSharingDetails : IMapFrom<ToDoList>
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Icon { get; set; }
    public bool IsOneTimeToggleDefault { get; set; }
    public ListSharingState SharingState { get; set; }
    public short? Order { get; set; }
    public List<TaskDto> Tasks { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<ToDoList, ListWithSharingDetails>()
            .ForMember(x => x.SharingState, opt => opt.MapFrom<ListSharingStateResolver>())
            .ForMember(x => x.Order, opt => opt.MapFrom<ListOrderResolver>());
    }
}