using System;
using AutoMapper;
using PersonalAssistant.Application.Mappings;
using PersonalAssistant.Domain.Entities.Common;
using PersonalAssistant.Domain.Entities.ToDoAssistant;

namespace PersonalAssistant.Application.Contracts.ToDoAssistant.Notifications.Models
{
    public class NotificationDto : IMapFrom<Notification>
    {
        public int Id { get; set; }
        public int? ListId { get; set; }
        public int? TaskId { get; set; }
        public string UserImageUri { get; set; }
        public string Message { get; set; }
        public bool IsSeen { get; set; }
        public DateTime CreatedDate { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Notification, NotificationDto>()
                .ForMember(x => x.UserImageUri, opt => opt.MapFrom(src => src.User.ImageUri));
        }
    }
}
