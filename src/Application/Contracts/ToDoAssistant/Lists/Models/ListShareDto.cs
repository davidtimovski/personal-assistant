using System;
using AutoMapper;
using Application.Mappings;
using Domain.Entities.Common;
using Domain.Entities.ToDoAssistant;

namespace Application.Contracts.ToDoAssistant.Lists.Models;

public class ListShareDto : IMapFrom<ListShare>
{
    public int UserId { get; set; }
    public string Email { get; set; }
    public string ImageUri { get; set; }
    public bool IsAdmin { get; set; }
    public bool? IsAccepted { get; set; }
    public DateTime CreatedDate { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<ListShare, ListShareDto>()
            .ForMember(x => x.Email, opt => opt.MapFrom(src => src.User.Email))
            .ForMember(x => x.ImageUri, opt => opt.MapFrom(src => src.User.ImageUri));
    }
}