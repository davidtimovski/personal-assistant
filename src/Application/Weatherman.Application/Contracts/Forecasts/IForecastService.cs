using Sentry;
using Weatherman.Application.Contracts.Forecasts.Models;

namespace Weatherman.Application.Contracts.Forecasts;

public interface IForecastService
{
    Task<ForecastResult> GetAsync(GetForecast parameters, ISpan metricsSpan);
}
