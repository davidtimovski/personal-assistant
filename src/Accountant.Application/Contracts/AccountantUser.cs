using Application.Contracts.Models;
using Application.Mappings;
using AutoMapper;
using Domain.Common;

namespace Accountant.Application.Contracts;

public class AccountantUser : UserDto, IMapFrom<User>
{
    public void Mapping(Profile profile)
    {
        profile.CreateMap<User, AccountantUser>();
    }
}
