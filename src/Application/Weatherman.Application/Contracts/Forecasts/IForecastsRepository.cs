using Application.Domain.Weatherman;
using Sentry;
using Weatherman.Application.Contracts.Forecasts.Models;

namespace Weatherman.Application.Contracts.Forecasts;

public interface IForecastsRepository
{
    Forecast Get(GetForecast parameters, ISpan metricsSpan);
    Task CreateAsync(Forecast model, ISpan metricsSpan);
    Task UpdateAsync(int id, DateTime lastUpdate, string data, ISpan metricsSpan);
}
