using Microsoft.AspNetCore.Mvc;
using Weatherman.Application.Contracts.Forecasts;
using Weatherman.Application.Contracts.Forecasts.Models;

namespace Weatherman.Api.Controllers;

[Route("api/[controller]")]
public class ForecastsController : Controller
{
    private readonly IForecastService _forecastService;

    public ForecastsController(IForecastService forecastService)
    {
        _forecastService = forecastService;
    }

    [HttpGet]
    public async Task<IActionResult> Get(float latitude, float longitude, string temperatureUnit, string precipitationUnit, string windSpeedUnit, DateTime time)
    {
        if (temperatureUnit == null || precipitationUnit == null || windSpeedUnit == null)
        {
            return BadRequest();
        }

        var parameters = new GetForecast
        {
            Latitude = latitude,
            Longitude = longitude,
            TemperatureUnit = temperatureUnit,
            PrecipitationUnit = precipitationUnit,
            WindSpeedUnit = windSpeedUnit,
            Time = time
        };
        var forecast = await _forecastService.GetAsync(parameters);

        return Ok(forecast);
    }
}
