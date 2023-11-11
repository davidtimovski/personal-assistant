using AutoMapper;
using Core.Application.Contracts;
using Core.Application.Contracts.Models;
using Core.Application.Entities;
using Core.Application.Utils;
using Microsoft.Extensions.Logging;
using Sentry;

namespace Core.Application.Services;

public class TooltipService : ITooltipService
{
    private readonly ITooltipsRepository _tooltipsRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<TooltipService> _logger;

    public TooltipService(
        ITooltipsRepository? tooltipsRepository,
        IMapper? mapper,
        ILogger<TooltipService>? logger)
    {
        _tooltipsRepository = ArgValidator.NotNull(tooltipsRepository);
        _mapper = ArgValidator.NotNull(mapper);
        _logger = ArgValidator.NotNull(logger);
    }

    public Result<IReadOnlyList<TooltipDto>> GetAll(string application, int userId, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(TooltipService)}.{nameof(GetAll)}");

        try
        {
            var tooltips = _tooltipsRepository.GetAll(application, userId, metric);

            var result = tooltips.Select(x => _mapper.Map<TooltipDto>(x)).ToList();

            return new(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(GetAll)}");
            return new();
        }
        finally
        {
            metric.Finish();
        }
    }

    public Result<TooltipDto> GetByKey(int userId, string key, string application, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(TooltipService)}.{nameof(GetByKey)}");

        try
        {
            Tooltip tooltip = _tooltipsRepository.GetByKey(userId, key, application, metric);
            var result = _mapper.Map<TooltipDto>(tooltip);

            return new(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(GetByKey)}");
            return new();
        }
        finally
        {
            metric.Finish();
        }
    }

    public async Task<Result> ToggleDismissedAsync(int userId, string key, string application, bool isDismissed, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        var metric = metricsSpan.StartChild($"{nameof(TooltipService)}.{nameof(ToggleDismissedAsync)}");

        try
        {
            await _tooltipsRepository.ToggleDismissedAsync(userId, key, application, isDismissed, metric, cancellationToken);
            return new(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(ToggleDismissedAsync)}");
            return new();
        }
        finally
        {
            metric.Finish();
        }
    }
}
