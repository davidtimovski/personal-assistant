using Application.Contracts;
using Application.Contracts.Models;
using AutoMapper;
using Domain.Common;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class TooltipService : ITooltipService
{
    private readonly ITooltipsRepository _tooltipsRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<TooltipService> _logger;

    public TooltipService(
        ITooltipsRepository tooltipsRepository,
        IMapper mapper,
        ILogger<TooltipService> logger)
    {
        _tooltipsRepository = tooltipsRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public IEnumerable<TooltipDto> GetAll(string application, int userId)
    {
        try
        {
            var tooltips = _tooltipsRepository.GetAll(application, userId);

            var tooltipDtos = tooltips.Select(x => _mapper.Map<TooltipDto>(x));

            return tooltipDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(GetAll)}");
            throw;
        }
    }

    public TooltipDto GetByKey(int userId, string key, string application)
    {
        try
        {
            Tooltip tooltip = _tooltipsRepository.GetByKey(userId, key, application);
            return _mapper.Map<TooltipDto>(tooltip);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(GetByKey)}");
            throw;
        }
    }

    public async Task ToggleDismissedAsync(int userId, string key, string application, bool isDismissed)
    {
        try
        {
            await _tooltipsRepository.ToggleDismissedAsync(userId, key, application, isDismissed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(ToggleDismissedAsync)}");
            throw;
        }
    }
}