using System;
using System.Threading.Tasks;
using Application.Contracts.Weatherman.Forecasts;
using Application.Contracts.Weatherman.Forecasts.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Weatherman;

[Authorize]
[EnableCors("AllowWeatherman")]
[Route("api/[controller]")]
public class ForecastsController : Controller
{
    private readonly IForecastService _forecastService;

    public ForecastsController(IForecastService forecastService)
    {
        _forecastService = forecastService;
    }

    [HttpGet]
    public async Task<IActionResult> Get(float latitude, float longitude, string temperatureUnit, string precipitationUnit, string windSpeedUnit, DateTime date)
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
            Date = date
        };
        var forecast = await _forecastService.GetAsync(parameters);

        return Ok(forecast);
    }
}
