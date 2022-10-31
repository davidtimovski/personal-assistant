using System.Collections.Generic;

namespace Application.Contracts.Weatherman.Forecasts.Models;

public class ForecastResult
{
    public short Temperature { get; set; }
    public short ApparentTemperature { get; set; }
    public short Precipitation { get; set; }
    public short WindSpeed { get; set; }
    public WeatherCode WeatherCode { get; set; }
    public bool IsNight { get; set; }
    public List<Daily> Daily { get; } = new List<Daily>(7);
    public List<HourlyForecast> Hourly { get; } = new List<HourlyForecast>(24);
}

public readonly record struct Daily(WeatherCode WeatherCode, short TemperatureMax, short TemperatureMin);

public readonly record struct HourlyForecast(short Hour, short Temperature, short ApparentTemperature, short Precipitation, short WindSpeed, WeatherCode WeatherCode, bool IsNight);
