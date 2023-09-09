using Weatherman.Application.Contracts.Forecasts.Models;

namespace Application.UnitTests.Builders;

internal class GetForecastBuilder
{
    private string? temperatureUnit;
    private string? precipitationUnit;
    private string? windSpeedUnit;

    internal GetForecastBuilder WithUnits()
    {
        temperatureUnit = "celsius";
        precipitationUnit = "mm";
        windSpeedUnit = "kmh";
        return this;
    }

    internal GetForecast Build()
    {
        return new GetForecast
        {
            Latitude = 0,
            Longitude = 0,
            TemperatureUnit = temperatureUnit,
            PrecipitationUnit = precipitationUnit,
            WindSpeedUnit = windSpeedUnit,
            Time = DateTime.Now
        };
    }
}
