using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Tasks.Models;
using PersonalAssistant.Application.Mappings;
using PersonalAssistant.Domain.Entities.ToDoAssistant;

namespace PersonalAssistant.Application.Contracts.ToDoAssistant.Lists.Models
{
    public class ListWithTasks : IMapFrom<ToDoList>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public bool IsOneTimeToggleDefault { get; set; }
        public SharingState SharingState { get; set; }
        public bool IsArchived { get; set; }

        public List<TaskDto> Tasks { get; set; } = new List<TaskDto>();
        public List<TaskDto> PrivateTasks { get; set; }
        public List<TaskDto> CompletedTasks { get; set; } = new List<TaskDto>();
        public List<TaskDto> CompletedPrivateTasks { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<ToDoList, ListWithTasks>()
                .ForMember(x => x.SharingState, opt => opt.MapFrom<SharingStateResolver>())
                .ForMember(x => x.IsArchived, opt => opt.MapFrom<IsArchivedResolver>())
                .ForMember(x => x.Tasks, opt => opt.MapFrom(src => src.Tasks.Where(x => !x.IsCompleted && !x.PrivateToUserId.HasValue).OrderBy(x => x.Order)))
                .ForMember(x => x.PrivateTasks, opt => opt.MapFrom(src => src.Tasks.Where(x => !x.IsCompleted && x.PrivateToUserId.HasValue).OrderBy(x => x.Order)))
                .ForMember(x => x.CompletedTasks, opt => opt.MapFrom(src => src.Tasks.Where(x => x.IsCompleted && !x.PrivateToUserId.HasValue).OrderBy(x => x.Order)))
                .ForMember(x => x.CompletedPrivateTasks, opt => opt.MapFrom(src => src.Tasks.Where(x => x.IsCompleted && x.PrivateToUserId.HasValue).OrderBy(x => x.Order)));
        }
    }
}
