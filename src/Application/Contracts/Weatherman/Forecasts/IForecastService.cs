using System.Threading.Tasks;
using Application.Contracts.Weatherman.Forecasts.Models;

namespace Application.Contracts.Weatherman.Forecasts;

public interface IForecastService
{
    Task<ForecastResult> GetAsync(GetForecast parameters);
}
