using AutoMapper;
using Core.Application.Entities;
using Core.Application.Mappings;

namespace Core.Application.Contracts.Models;

public class UserDto
{
    public string Email { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Language { get; set; } = null!;
    public string Culture { get; set; } = null!;
    public string ImageUri { get; set; } = null!;
}

public class ToDoAssistantUser : UserDto, IMapFrom<User>
{
    public bool ToDoNotificationsEnabled { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<User, ToDoAssistantUser>();
    }
}

public class ChefUser : UserDto, IMapFrom<User>
{
    public bool ChefNotificationsEnabled { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<User, ChefUser>();
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
