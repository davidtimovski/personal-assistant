using Application.Mappings;
using AutoMapper;
using Domain.Common;

namespace Application.Contracts.Models;

public class TooltipDto : IMapFrom<Tooltip>
{
    public string Key { get; set; }
    public bool IsDismissed { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Tooltip, TooltipDto>();
    }
}