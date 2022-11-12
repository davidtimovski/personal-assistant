using System.Collections.Generic;

namespace Application.Contracts.Weatherman.Forecasts.Models;

public class ForecastResult
{
    public WeatherCode WeatherCode { get; set; }
    public short Temperature { get; set; }
    public short ApparentTemperature { get; set; }
    public float Precipitation { get; set; }
    public short WindSpeed { get; set; }
    public bool IsNight { get; set; }
    public List<HourlyForecast> Hourly { get; set; }
    public List<DailyForecast> NextDays { get; set; }
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

public readonly record struct HourlyForecast(short Hour, WeatherCode WeatherCode, short Temperature, short ApparentTemperature, float Precipitation, short WindSpeed, bool IsNight);
