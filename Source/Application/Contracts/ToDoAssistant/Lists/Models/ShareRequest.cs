using AutoMapper;
using PersonalAssistant.Application.Mappings;
using PersonalAssistant.Domain.Entities.ToDoAssistant;

namespace PersonalAssistant.Application.Contracts.ToDoAssistant.Lists.Models
{
    public class ShareRequest : IMapFrom<Share>
    {
        public int ListId { get; set; }
        public string ListName { get; set; }
        public string ListOwnerName { get; set; }
        public bool? IsAccepted { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Share, ShareRequest>()
                .ForMember(x => x.ListName, opt => opt.MapFrom(src => src.List.Name))
                .ForMember(x => x.ListOwnerName, opt => opt.MapFrom(src => src.User.Name));
        }
    }
}
