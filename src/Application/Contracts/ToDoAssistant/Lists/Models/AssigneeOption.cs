using AutoMapper;
using PersonalAssistant.Application.Mappings;
using PersonalAssistant.Domain.Entities;
using PersonalAssistant.Domain.Entities.Common;

namespace PersonalAssistant.Application.Contracts.ToDoAssistant.Lists.Models
{
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
}
