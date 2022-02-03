using AutoMapper;
using Application.Mappings;
using Domain.Entities;
using Domain.Entities.Common;

namespace Application.Contracts.ToDoAssistant.Lists.Models;

public class AssigneeOption : IMapFrom<User>
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string ImageUri { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<User, AssigneeOption>();
    }
}