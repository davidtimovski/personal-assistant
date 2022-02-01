using AutoMapper;
using Application.Mappings;
using Domain.Entities;
using Domain.Entities.Common;

namespace Application.Contracts.Common.Models
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
