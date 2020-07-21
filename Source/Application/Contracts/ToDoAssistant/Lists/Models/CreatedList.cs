using AutoMapper;
using PersonalAssistant.Application.Mappings;
using PersonalAssistant.Domain.Entities.ToDoAssistant;

namespace PersonalAssistant.Application.Contracts.ToDoAssistant.Lists.Models
{
    public class CreatedList : IMapFrom<ToDoList>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public bool IsOneTimeToggleDefault { get; set; }
        public SharingState SharingState { get; set; }
        public short? Order { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<ToDoList, CreatedList>()
                .ForMember(x => x.SharingState, opt => opt.MapFrom<SharingStateResolver>())
                .ForMember(x => x.Order, opt => opt.MapFrom<ListOrderResolver>());
        }
    }
}
