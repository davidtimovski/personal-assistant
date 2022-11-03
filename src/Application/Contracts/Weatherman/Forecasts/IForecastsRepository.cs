using System;
using System.Threading.Tasks;
using Application.Contracts.Weatherman.Forecasts.Models;
using Domain.Entities.Weatherman;

namespace Application.Contracts.Weatherman.Forecasts;

public interface IForecastsRepository
{
    Forecast Get(GetForecast parameters);
    Task CreateAsync(Forecast model);
    Task UpdateAsync(int id, DateTime lastUpdate, string data);
}
