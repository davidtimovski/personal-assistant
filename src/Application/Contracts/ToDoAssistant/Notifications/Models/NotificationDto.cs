using System;
using AutoMapper;
using Application.Mappings;
using Domain.Entities.Common;
using Domain.Entities.ToDoAssistant;

namespace Application.Contracts.ToDoAssistant.Notifications.Models
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
