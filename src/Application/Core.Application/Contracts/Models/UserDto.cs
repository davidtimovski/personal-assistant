using Application.Domain.Common;
using AutoMapper;
using Core.Application.Mappings;

namespace Core.Application.Contracts.Models;

public class UserDto
{
    public string Email { get; set; }
    public string Name { get; set; }
    public string Language { get; set; }
    public string Culture { get; set; }
    public string ImageUri { get; set; }
}

public class ToDoAssistantUser : UserDto, IMapFrom<User>
{
    public bool ToDoNotificationsEnabled { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<User, ToDoAssistantUser>();
    }
}

public class AccountantUser : UserDto, IMapFrom<User>
{
    public void Mapping(Profile profile)
    {
        profile.CreateMap<User, AccountantUser>();
    }
}

public class WeathermanUser : UserDto, IMapFrom<User>
{
    public void Mapping(Profile profile)
    {
        profile.CreateMap<User, WeathermanUser>();
    }
}

public class TrainerUser : UserDto, IMapFrom<User>
{
    public void Mapping(Profile profile)
    {
        profile.CreateMap<User, TrainerUser>();
    }
}
