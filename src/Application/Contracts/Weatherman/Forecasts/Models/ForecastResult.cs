using System.Collections.Generic;

namespace Application.Contracts.Weatherman.Forecasts.Models;

public class ForecastResult
{
    public WeatherCode WeatherCode { get; init; }
    public short Temperature { get; init; }
    public short ApparentTemperature { get; init; }
    public float Precipitation { get; init; }
    public short WindSpeed { get; init; }
    public bool IsNight { get; init; }
    public List<HourlyForecast> Hourly { get; init; }
    public List<DailyForecast> NextDays { get; init; }
}

public class DailyForecast
{
    public string Date { get; init; }
    public WeatherCode WeatherCode { get; init; }
    public short TemperatureMax { get; init; }
    public short TemperatureMin { get; init; }
    public float Precipitation { get; init; }
    public List<HourlyForecast> Hourly { get; init; }
}

public readonly record struct HourlyForecast(short Hour, WeatherCode WeatherCode, short Temperature, short ApparentTemperature, bool IsNight);
