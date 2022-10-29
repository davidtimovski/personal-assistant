using System.Collections.Generic;

namespace Application.Contracts.Weatherman.Forecasts.Models;

public class ForecastResult
{
    public short Temperature { get; set; }
    public short Precipitation { get; set; }
    public short WindSpeed { get; set; }
    public WeatherCode WeatherCode { get; set; }
    public List<HourlyForecast> Hourly { get; } = new List<HourlyForecast>(24);
}

public readonly record struct HourlyForecast(string Time, short Temperature, short Precipitation, short WindSpeed, WeatherCode WeatherCode);
