using AutoMapper;
using Core.Application.Contracts;
using Core.Application.Contracts.Models;
using Core.Application.Entities;
using Microsoft.Extensions.Logging;
using Sentry;

namespace Core.Application.Services;

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

    public IEnumerable<TooltipDto> GetAll(string application, int userId, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(TooltipService)}.{nameof(GetAll)}");

        try
        {
            var tooltips = _tooltipsRepository.GetAll(application, userId, metric);

            var tooltipDtos = tooltips.Select(x => _mapper.Map<TooltipDto>(x));

            return tooltipDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(GetAll)}");
            throw;
        }
        finally
        {
            metric.Finish();
        }
    }

    public TooltipDto GetByKey(int userId, string key, string application, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(TooltipService)}.{nameof(GetByKey)}");

        try
        {
            Tooltip tooltip = _tooltipsRepository.GetByKey(userId, key, application, metric);
            return _mapper.Map<TooltipDto>(tooltip);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(GetByKey)}");
            throw;
        }
        finally
        {
            metric.Finish();
        }
    }

    public async Task ToggleDismissedAsync(int userId, string key, string application, bool isDismissed, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(TooltipService)}.{nameof(ToggleDismissedAsync)}");

        try
        {
            await _tooltipsRepository.ToggleDismissedAsync(userId, key, application, isDismissed, metric);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(ToggleDismissedAsync)}");
            throw;
        }
        finally
        {
            metric.Finish();
        }
    }
}