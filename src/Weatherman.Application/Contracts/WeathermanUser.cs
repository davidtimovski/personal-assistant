using Application.Contracts.Models;
using Application.Mappings;
using AutoMapper;
using Domain.Common;

namespace Weatherman.Application.Contracts;

public class WeathermanUser : UserDto, IMapFrom<User>
{
    public void Mapping(Profile profile)
    {
        profile.CreateMap<User, WeathermanUser>();
    }
}
