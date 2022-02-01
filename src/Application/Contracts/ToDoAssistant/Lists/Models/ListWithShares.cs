using System.Collections.Generic;
using AutoMapper;
using Application.Mappings;
using Domain.Entities.Common;
using Domain.Entities.ToDoAssistant;

namespace Application.Contracts.ToDoAssistant.Lists.Models
{
    public class ListWithShares : IMapFrom<ToDoList>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ListSharingState SharingState { get; set; }
        public string OwnerEmail { get; set; }
        public string OwnerImageUri { get; set; }
        public ListShareDto UserShare { get; set; }

        public List<ListShareDto> Shares { get; set; } = new List<ListShareDto>();

        public void Mapping(Profile profile)
        {
            profile.CreateMap<ToDoList, ListWithShares>()
                .ForMember(x => x.SharingState, opt => opt.MapFrom<ListSharingStateResolver>())
                .ForMember(x => x.OwnerEmail, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(x => x.OwnerImageUri, opt => opt.MapFrom(src => src.User.ImageUri))
                .ForMember(x => x.UserShare, opt => opt.MapFrom<ListWithSharesUserShareResolver>());
        }
    }
}
