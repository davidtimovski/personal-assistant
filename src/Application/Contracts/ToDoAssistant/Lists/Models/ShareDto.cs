using System;
using AutoMapper;
using PersonalAssistant.Application.Mappings;
using PersonalAssistant.Domain.Entities.Common;
using PersonalAssistant.Domain.Entities.ToDoAssistant;

namespace PersonalAssistant.Application.Contracts.ToDoAssistant.Lists.Models
{
    public class ShareDto : IMapFrom<Share>
    {
        public int UserId { get; set; }
        public string Email { get; set; }
        public string ImageUri { get; set; }
        public bool IsAdmin { get; set; }
        public bool? IsAccepted { get; set; }
        public DateTime CreatedDate { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Share, ShareDto>()
                .ForMember(x => x.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(x => x.ImageUri, opt => opt.MapFrom(src => src.User.ImageUri));
        }
    }
}
