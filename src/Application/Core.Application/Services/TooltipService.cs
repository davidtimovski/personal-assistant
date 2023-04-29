using Application.Domain.Common;
using AutoMapper;
using Core.Application.Contracts;
using Core.Application.Contracts.Models;
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

    public IEnumerable<TooltipDto> GetAll(string application, int userId, ITransaction tr)
    {
        var span = tr.StartChild($"{nameof(TooltipService)}.{nameof(GetAll)}");

        try
        {
            var tooltips = _tooltipsRepository.GetAll(application, userId, tr);

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
            span.Finish();
        }
    }

    public TooltipDto GetByKey(int userId, string key, string application, ITransaction tr)
    {
        var span = tr.StartChild($"{nameof(TooltipService)}.{nameof(GetByKey)}");

        try
        {
            Tooltip tooltip = _tooltipsRepository.GetByKey(userId, key, application, tr);
            return _mapper.Map<TooltipDto>(tooltip);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(GetByKey)}");
            throw;
        }
        finally
        {
            span.Finish();
        }
    }

    public async Task ToggleDismissedAsync(int userId, string key, string application, bool isDismissed, ITransaction tr)
    {
        var span = tr.StartChild($"{nameof(TooltipService)}.{nameof(ToggleDismissedAsync)}");

        try
        {
            await _tooltipsRepository.ToggleDismissedAsync(userId, key, application, isDismissed, tr);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(ToggleDismissedAsync)}");
            throw;
        }
        finally
        {
            span.Finish();
        }
    }
}