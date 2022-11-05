using Application.Contracts.Common.Models;
using Application.Mappings;
using AutoMapper;
using Domain.Entities.Common;

namespace Application.Contracts.Accountant;

public class AccountantUser : UserDto, IMapFrom<User>
{
    public void Mapping(Profile profile)
    {
        profile.CreateMap<User, AccountantUser>();
    }
}
