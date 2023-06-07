using AutoMapper;
using Core.Application.Mappings;
using ToDoAssistant.Application.Entities;

namespace ToDoAssistant.Application.Contracts.Notifications.Models;

public class NotificationDto : IMapFrom<Notification>
{
    public int Id { get; set; }
    public int? ListId { get; set; }
    public int? TaskId { get; set; }
    public string UserName { get; set; } = null!;
    public string UserImageUri { get; set; } = null!;
    public string Message { get; set; } = null!;
    public bool IsSeen { get; set; }
    public DateTime CreatedDate { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Notification, NotificationDto>()
            .ForMember(x => x.UserName, opt => opt.MapFrom(src => src.User.Name))
            .ForMember(x => x.UserImageUri, opt => opt.MapFrom(src => src.User.ImageUri));
    }
}
