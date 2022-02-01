using System.Linq;
using AutoMapper;
using Application.Mappings;
using Domain.Entities.ToDoAssistant;

namespace Application.Contracts.ToDoAssistant.Lists.Models
{
    public class ToDoListOption : IMapFrom<ToDoList>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsShared { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<ToDoList, ToDoListOption>()
                .ForMember(x => x.IsShared, opt => opt.MapFrom(x => x.Shares.Any()));
        }
    }
}
