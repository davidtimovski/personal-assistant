using AutoMapper;
using Core.Application.Mappings;
using ToDoAssistant.Application.Entities;

namespace ToDoAssistant.Application.Contracts.Lists.Models;

public class ShareListRequest : IMapFrom<ListShare>
{
    public int ListId { get; set; }
    public string ListName { get; set; } = null!;
    public string ListOwnerName { get; set; } = null!;
    public bool? IsAccepted { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<ListShare, ShareListRequest>()
            .ForMember(x => x.ListName, opt => opt.MapFrom(src => src.List!.Name))
            .ForMember(x => x.ListOwnerName, opt => opt.MapFrom(src => src.User!.Name));
    }
}
