using Application.Contracts.Common.Models;
using Application.Mappings;
using AutoMapper;
using Domain.Entities.Common;

namespace Application.Contracts.ToDoAssistant;

public class ToDoAssistantUser : UserDto, IMapFrom<User>
{
    public bool ToDoNotificationsEnabled { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<User, ToDoAssistantUser>();
    }
}
