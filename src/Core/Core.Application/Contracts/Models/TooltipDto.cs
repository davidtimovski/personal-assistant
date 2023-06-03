using AutoMapper;
using Core.Application.Entities;
using Core.Application.Mappings;

namespace Core.Application.Contracts.Models;

public class TooltipDto : IMapFrom<Tooltip>
{
    public string Key { get; set; } = null!;
    public bool IsDismissed { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Tooltip, TooltipDto>();
    }
}
