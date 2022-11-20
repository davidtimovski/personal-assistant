using Domain.Weatherman;
using Weatherman.Application.Contracts.Forecasts.Models;

namespace Weatherman.Application.Contracts.Forecasts;

public interface IForecastsRepository
{
    Forecast Get(GetForecast parameters);
    Task CreateAsync(Forecast model);
    Task UpdateAsync(int id, DateTime lastUpdate, string data);
}
