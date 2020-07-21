using AutoMapper;
using PersonalAssistant.Application.Mappings;
using PersonalAssistant.Domain.Entities;
using PersonalAssistant.Domain.Entities.Common;

namespace PersonalAssistant.Application.Contracts.Common.Models
{
    public class TooltipDto : IMapFrom<Tooltip>
    {
        public string Key { get; set; }
        public bool IsDismissed { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Tooltip, TooltipDto>();
        }
    }
}
