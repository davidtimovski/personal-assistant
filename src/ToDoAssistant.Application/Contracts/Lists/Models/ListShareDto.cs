using Application.Mappings;
using AutoMapper;
using Domain.ToDoAssistant;

namespace ToDoAssistant.Application.Contracts.Lists.Models;

public class ListShareDto : IMapFrom<ListShare>
{
    public int UserId { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
    public string ImageUri { get; set; }
    public bool IsAdmin { get; set; }
    public bool? IsAccepted { get; set; }
    public DateTime CreatedDate { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<ListShare, ListShareDto>()
            .ForMember(x => x.Email, opt => opt.MapFrom(src => src.User.Email))
            .ForMember(x => x.Name, opt => opt.MapFrom(src => src.User.Name))
            .ForMember(x => x.ImageUri, opt => opt.MapFrom(src => src.User.ImageUri));
    }
}
