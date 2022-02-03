using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Application.Contracts.Common;
using Application.Contracts.Common.Models;
using Domain.Entities.Common;

namespace Application.Services.Common;

public class TooltipService : ITooltipService
{
    private readonly ITooltipsRepository _tooltipsRepository;
    private readonly IMapper _mapper;

    public TooltipService(
        ITooltipsRepository tooltipsRepository,
        IMapper mapper)
    {
        _tooltipsRepository = tooltipsRepository;
        _mapper = mapper;
    }

    public IEnumerable<TooltipDto> GetAll(string application, int userId)
    {
        var tooltips = _tooltipsRepository.GetAll(application, userId);

        var tooltipDtos = tooltips.Select(x => _mapper.Map<TooltipDto>(x));

        return tooltipDtos;
    }

    public TooltipDto GetByKey(int userId, string key, string application)
    {
        Tooltip tooltip = _tooltipsRepository.GetByKey(userId, key, application);
        return _mapper.Map<TooltipDto>(tooltip);
    }

    public async Task ToggleDismissedAsync(int userId, string key, string application, bool isDismissed)
    {
        await _tooltipsRepository.ToggleDismissedAsync(userId, key, application, isDismissed);
    }
}