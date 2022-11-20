using Weatherman.Application.Contracts.Forecasts.Models;

namespace Application.UnitTests.Builders;

public class GetForecastBuilder
{
    private string temperatureUnit;
    private string precipitationUnit;
    private string windSpeedUnit;

    public GetForecastBuilder WithUnits()
    {
        temperatureUnit = "celsius";
        precipitationUnit = "mm";
        windSpeedUnit = "kmh";
        return this;
    }

    public GetForecast Build()
    {
        return new GetForecast
        {
            TemperatureUnit = temperatureUnit,
            PrecipitationUnit = precipitationUnit,
            WindSpeedUnit = windSpeedUnit
        };
    }
}
