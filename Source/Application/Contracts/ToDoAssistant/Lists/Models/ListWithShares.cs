using System.Collections.Generic;
using AutoMapper;
using PersonalAssistant.Application.Mappings;
using PersonalAssistant.Domain.Entities.Common;
using PersonalAssistant.Domain.Entities.ToDoAssistant;

namespace PersonalAssistant.Application.Contracts.ToDoAssistant.Lists.Models
{
    public class ListWithShares : IMapFrom<ToDoList>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public SharingState SharingState { get; set; }
        public string OwnerEmail { get; set; }
        public string OwnerImageUri { get; set; }
        public ShareDto UserShare { get; set; }

        public List<ShareDto> Shares { get; set; } = new List<ShareDto>();

        public void Mapping(Profile profile)
        {
            profile.CreateMap<ToDoList, ListWithShares>()
                .ForMember(x => x.SharingState, opt => opt.MapFrom<SharingStateResolver>())
                .ForMember(x => x.OwnerEmail, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(x => x.OwnerImageUri, opt => opt.MapFrom(src => src.User.ImageUri))
                .ForMember(x => x.UserShare, opt => opt.MapFrom<ListWithSharesUserShareResolver>());
        }
    }
}
