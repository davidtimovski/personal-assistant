using Application.Contracts.Common.Models;
using Application.Mappings;
using AutoMapper;
using Domain.Entities.Common;

namespace Application.Contracts.Weatherman;

public class WeathermanUser : UserDto, IMapFrom<User>
{
    public void Mapping(Profile profile)
    {
        profile.CreateMap<User, WeathermanUser>();
    }
}
