using AutoMapper;
using PersonalAssistant.Application.Mappings;
using PersonalAssistant.Domain.Entities.ToDoAssistant;

namespace PersonalAssistant.Application.Contracts.ToDoAssistant.Tasks.Models
{
    public class SimpleTask : IMapFrom<ToDoTask>
    {
        public int Id { get; set; }
        public int ListId { get; set; }
        public string Name { get; set; }
        public int? PrivateToUserId { get; set; }
        public int? AssignedToUserId { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<ToDoTask, SimpleTask>();
        }
    }
}
