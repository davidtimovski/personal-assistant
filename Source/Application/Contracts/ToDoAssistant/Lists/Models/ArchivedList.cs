using AutoMapper;
using PersonalAssistant.Application.Mappings;
using PersonalAssistant.Domain.Entities.ToDoAssistant;

namespace PersonalAssistant.Application.Contracts.ToDoAssistant.Lists.Models
{
    public class ArchivedList : IMapFrom<ToDoList>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public SharingState SharingState { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<ToDoList, ArchivedList>()
                .ForMember(x => x.SharingState, opt => opt.MapFrom<SharingStateResolver>());
        }
    }
}
