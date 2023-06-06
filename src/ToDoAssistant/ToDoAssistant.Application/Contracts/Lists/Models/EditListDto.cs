using AutoMapper;
using Core.Application.Mappings;
using ToDoAssistant.Application.Entities;
using ToDoAssistant.Application.Mappings;

namespace ToDoAssistant.Application.Contracts.Lists.Models;

public class EditListDto : IMapFrom<ToDoList>
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Icon { get; set; } = null!;
    public bool IsOneTimeToggleDefault { get; set; }
    public bool NotificationsEnabled { get; set; }
    public ListSharingState SharingState { get; set; }
    public short? Order { get; set; }
    public bool IsArchived { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<ToDoList, EditListDto>()
            .ForMember(x => x.NotificationsEnabled, opt => opt.MapFrom<ListNotificationsEnabledResolver>())
            .ForMember(x => x.SharingState, opt => opt.MapFrom<ListSharingStateResolver>())
            .ForMember(x => x.Order, opt => opt.MapFrom<ListOrderResolver>())
            .ForMember(x => x.IsArchived, opt => opt.MapFrom<IsArchivedResolver>());
    }
}
