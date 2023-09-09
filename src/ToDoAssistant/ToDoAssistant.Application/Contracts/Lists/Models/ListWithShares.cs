using AutoMapper;
using Core.Application.Mappings;
using ToDoAssistant.Application.Entities;
using ToDoAssistant.Application.Mappings;

namespace ToDoAssistant.Application.Contracts.Lists.Models;

public class ListWithShares : IMapFrom<ToDoList>
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public ListSharingState SharingState { get; set; }
    public string OwnerEmail { get; set; } = null!;
    public string OwnerName { get; set; } = null!;
    public string OwnerImageUri { get; set; } = null!;
    public ListShareDto? UserShare { get; set; }

    public List<ListShareDto> Shares { get; set; } = new();

    public void Mapping(Profile profile)
    {
        profile.CreateMap<ToDoList, ListWithShares>()
            .ForMember(x => x.SharingState, opt => opt.MapFrom<ListSharingStateResolver>())
            .ForMember(x => x.OwnerEmail, opt => opt.MapFrom(src => src.User!.Email))
            .ForMember(x => x.OwnerName, opt => opt.MapFrom(src => src.User!.Name))
            .ForMember(x => x.OwnerImageUri, opt => opt.MapFrom(src => src.User!.ImageUri))
            .ForMember(x => x.UserShare, opt => opt.MapFrom<ListWithSharesUserShareResolver>());
    }
}
