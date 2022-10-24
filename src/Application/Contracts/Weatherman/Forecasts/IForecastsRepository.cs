using System;
using System.Threading.Tasks;
using Domain.Entities.Weatherman;

namespace Application.Contracts.Weatherman.Forecasts;

public interface IForecastsRepository
{
    Forecast Get(float latitude, float longitude);
    Task CreateAsync(Forecast model);
    Task UpdateAsync(int id, DateTime lastUpdate, string data);
}
