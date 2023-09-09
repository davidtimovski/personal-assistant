namespace Weatherman.Application.Contracts.Forecasts.Models;

public class ForecastResult
{
    public required WeatherCode WeatherCode { get; set; }
    public required short Temperature { get; set; }
    public required short ApparentTemperature { get; set; }
    public required float Precipitation { get; set; }
    public required short WindSpeed { get; set; }
    public required TimeOfDay TimeOfDay { get; set; }
    public required List<HourlyForecast> Hourly { get; set; }
    public required List<DailyForecast> NextDays { get; set; }
}

public class DailyForecast
{
    public required string Date { get; init; }
    public required WeatherCode WeatherCode { get; init; }
    public required short TemperatureMax { get; init; }
    public required short TemperatureMin { get; init; }
    public required float Precipitation { get; init; }
    public required List<HourlyForecast> Hourly { get; init; }
}

public readonly record struct HourlyForecast(short Hour, WeatherCode WeatherCode, short Temperature, short ApparentTemperature, float Precipitation, short WindSpeed, TimeOfDay TimeOfDay);

public enum TimeOfDay
{
    Day,
    SunLow,
    SunLower,
    Night
}
