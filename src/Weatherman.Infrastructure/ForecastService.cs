using System.Text.Json;
using System.Web;
using Domain.Weatherman;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Weatherman.Application.Contracts.Forecasts;
using Weatherman.Application.Contracts.Forecasts.Models;
using Weatherman.Infrastructure.Models;

namespace Weatherman.Infrastructure;

public class ForecastService : IForecastService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IForecastsRepository _forecastsRepository;
    private readonly ILogger<ForecastService> _logger;
    private readonly HashSet<string> _validTemperatureUnits = new()
    {
        "celsius",
        "fahrenheit"
    };
    private readonly HashSet<string> _validPrecipitationUnits = new()
    {
        "mm",
        "inch"
    };
    private readonly HashSet<string> _validWindSpeedUnits = new()
    {
        "kmh",
        "mph"
    };

    public ForecastService(
        IHttpClientFactory httpClientFactory,
        IForecastsRepository forecastsRepository,
        ILogger<ForecastService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _forecastsRepository = forecastsRepository;
        _logger = logger;
    }

    public async Task<ForecastResult> GetAsync(GetForecast parameters)
    {
        var now = DateTime.UtcNow;

        if (!_validTemperatureUnits.Contains(parameters.TemperatureUnit))
        {
            throw new ValidationException("Forecasts.InvalidTemperatureUnit");
        }

        if (!_validPrecipitationUnits.Contains(parameters.PrecipitationUnit))
        {
            throw new ValidationException("Forecasts.InvalidPrecipitationUnit");
        }

        if (!_validWindSpeedUnits.Contains(parameters.WindSpeedUnit))
        {
            throw new ValidationException("Forecasts.InvalidWindSpeedUnit");
        }

        try
        {
            parameters.Latitude = (float)Math.Round(parameters.Latitude, 2);
            parameters.Longitude = (float)Math.Round(parameters.Longitude, 2);

            var forecast = _forecastsRepository.Get(parameters);
            if (forecast == null)
            {
                string data = await GetFromProviderAsync(parameters);
                var openMeteoResult = JsonSerializer.Deserialize<OpenMeteoResult>(data);
                ForecastResult result = Map(openMeteoResult, parameters);

                forecast = new Forecast
                {
                    Latitude = parameters.Latitude,
                    Longitude = parameters.Longitude,
                    TemperatureUnit = parameters.TemperatureUnit,
                    PrecipitationUnit = parameters.PrecipitationUnit,
                    WindSpeedUnit = parameters.WindSpeedUnit,
                    LastUpdate = now,
                    Data = JsonSerializer.Serialize(result)
                };

                await _forecastsRepository.CreateAsync(forecast);

                return result;
            }
            else if (forecast.LastUpdate.Day != now.Day || forecast.LastUpdate < now.AddMinutes(-30))
            {
                string data = await GetFromProviderAsync(parameters);
                var openMeteoResult = JsonSerializer.Deserialize<OpenMeteoResult>(data);
                ForecastResult result = Map(openMeteoResult, parameters);

                forecast.Data = JsonSerializer.Serialize(result);
                await _forecastsRepository.UpdateAsync(forecast.Id, now, forecast.Data);

                return result;
            }

            var cachedResult = JsonSerializer.Deserialize<ForecastResult>(forecast.Data);

            if (forecast.LastUpdate.Hour != now.Hour)
            {
                HourlyForecast nextHour = cachedResult.Hourly[0];

                cachedResult.Temperature = nextHour.Temperature;
                cachedResult.ApparentTemperature = nextHour.ApparentTemperature;
                cachedResult.Precipitation = nextHour.Precipitation;
                cachedResult.WindSpeed = nextHour.WindSpeed;
                cachedResult.TimeOfDay = nextHour.TimeOfDay;
                cachedResult.Hourly.RemoveAt(0);

                forecast.Data = JsonSerializer.Serialize(cachedResult);
                await _forecastsRepository.UpdateAsync(forecast.Id, now, forecast.Data);
            }

            return cachedResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(GetAsync)}");
            throw;
        }
    }

    private async Task<string> GetFromProviderAsync(GetForecast parameters)
    {
        var queryString = HttpUtility.ParseQueryString(string.Empty);
        queryString.Add("latitude", parameters.Latitude.ToString());
        queryString.Add("longitude", parameters.Longitude.ToString());
        queryString.Add("temperature_unit", parameters.TemperatureUnit);
        queryString.Add("precipitation_unit", parameters.PrecipitationUnit);
        queryString.Add("windspeed_unit", parameters.WindSpeedUnit);
        queryString.Add("hourly", "weathercode,temperature_2m,apparent_temperature,precipitation,windspeed_10m");
        queryString.Add("daily", "weathercode,temperature_2m_max,temperature_2m_min,precipitation_sum,sunrise,sunset");
        queryString.Add("timezone", "auto");

        using HttpClient httpClient = _httpClientFactory.CreateClient("open-meteo");
        httpClient.BaseAddress = new Uri("https://api.open-meteo.com/v1/");
        HttpResponseMessage response = await httpClient.GetAsync("forecast?" + HttpUtility.UrlDecode(queryString.ToString()));

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }

    private static ForecastResult Map(OpenMeteoResult openMeteoResult, GetForecast parameters)
    {
        var firstDaySunrise = openMeteoResult.daily.sunrise[0].TimeOfDay;
        var firstDaySunset = openMeteoResult.daily.sunset[0].TimeOfDay;
        var result = new ForecastResult
        {
            WeatherCode = (WeatherCode)openMeteoResult.hourly.weathercode[parameters.Time.Hour],
            Temperature = (short)Math.Round(openMeteoResult.hourly.temperature_2m[parameters.Time.Hour]),
            ApparentTemperature = (short)Math.Round(openMeteoResult.hourly.apparent_temperature[parameters.Time.Hour]),
            Precipitation = (float)Math.Round(openMeteoResult.hourly.precipitation[parameters.Time.Hour], 1),
            WindSpeed = (short)Math.Round(openMeteoResult.hourly.windspeed_10m[parameters.Time.Hour]),
            TimeOfDay = GetTimeOfDay(parameters.Time.TimeOfDay, firstDaySunrise, firstDaySunset),
            NextDays = new List<DailyForecast>(5),
            Hourly = new List<HourlyForecast>(24)
        };

        int from = parameters.Time.Hour + 1;
        int to = from + 24;
        for (var i = from; i < to; i++)
        {
            result.Hourly.Add(new HourlyForecast(
                Hour: (short)openMeteoResult.hourly.time[i].Hour,
                WeatherCode: (WeatherCode)openMeteoResult.hourly.weathercode[i],
                Temperature: (short)Math.Round(openMeteoResult.hourly.temperature_2m[i]),
                ApparentTemperature: (short)Math.Round(openMeteoResult.hourly.apparent_temperature[i]),
                Precipitation: (float)Math.Round(openMeteoResult.hourly.precipitation[i], 1),
                WindSpeed: (short)Math.Round(openMeteoResult.hourly.windspeed_10m[i]),
                TimeOfDay: GetTimeOfDay(openMeteoResult.hourly.time[i].TimeOfDay, firstDaySunrise, firstDaySunset)
            ));
        }

        for (var i = 1; i < 6; i++)
        {
            var dailyForecast = new DailyForecast
            {
                Date = openMeteoResult.daily.time[i].ToString("yyyy-MM-dd"),
                WeatherCode = (WeatherCode)openMeteoResult.daily.weathercode[i],
                TemperatureMax = (short)Math.Round(openMeteoResult.daily.temperature_2m_max[i]),
                TemperatureMin = (short)Math.Round(openMeteoResult.daily.temperature_2m_min[i]),
                Precipitation = (float)Math.Round(openMeteoResult.daily.precipitation_sum[i], 1),
                Hourly = new List<HourlyForecast>(24)
            };

            var sunrise = openMeteoResult.daily.sunrise[i].TimeOfDay;
            var sunset = openMeteoResult.daily.sunset[i].TimeOfDay;

            var fromIndex = i * 24 + 7;
            var toIndex = fromIndex + 24;
            for (var j = fromIndex; j < toIndex; j++)
            {
                dailyForecast.Hourly.Add(new HourlyForecast(
                   Hour: (short)openMeteoResult.hourly.time[j].Hour,
                   WeatherCode: (WeatherCode)openMeteoResult.hourly.weathercode[j],
                   Temperature: (short)Math.Round(openMeteoResult.hourly.temperature_2m[j]),
                   ApparentTemperature: (short)Math.Round(openMeteoResult.hourly.apparent_temperature[j]),
                   Precipitation: (float)Math.Round(openMeteoResult.hourly.precipitation[j], 1),
                   WindSpeed: (short)Math.Round(openMeteoResult.hourly.windspeed_10m[j]),
                   TimeOfDay: GetTimeOfDay(openMeteoResult.hourly.time[j].TimeOfDay, sunrise, sunset)
               ));
            }

            result.NextDays.Add(dailyForecast);
        }

        return result;
    }

    private static TimeOfDay GetTimeOfDay(TimeSpan time, TimeSpan sunrise, TimeSpan sunset)
    {
        if (time.Hours == sunrise.Hours || time.Hours == sunset.Hours)
        {
            return TimeOfDay.SunLower;
        }

        if (time.Hours == (sunrise.Hours + 1) || time.Hours == (sunset.Hours - 1))
        {
            return TimeOfDay.SunLow;
        }

        if (time < sunrise || time > sunset)
        {
            return TimeOfDay.Night;
        }

        return TimeOfDay.Day;
    }
}
