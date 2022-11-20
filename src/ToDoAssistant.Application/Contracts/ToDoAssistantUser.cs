using Application.Contracts.Models;
using Application.Mappings;
using AutoMapper;
using Domain.Common;

namespace ToDoAssistant.Application.Contracts;

public class ToDoAssistantUser : UserDto, IMapFrom<User>
{
    public bool ToDoNotificationsEnabled { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<User, ToDoAssistantUser>();
    }
}
