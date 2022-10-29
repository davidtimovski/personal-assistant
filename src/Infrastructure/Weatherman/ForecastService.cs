using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using Application.Contracts.Weatherman.Forecasts;
using Application.Contracts.Weatherman.Forecasts.Models;
using Domain.Entities.Weatherman;
using FluentValidation;
using Infrastructure.Weatherman.Models;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Weatherman;

public class ForecastService : IForecastService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IForecastsRepository _forecastsRepository;
    private readonly ILogger<ForecastService> _logger;
    private readonly HashSet<string> _validTemperatureUnits = new HashSet<string>
    {
        "celsius",
        "fahrenheit"
    };
    private readonly HashSet<string> _validPrecipitationUnits = new HashSet<string>
    {
        "mm",
        "inch"
    };
    private readonly HashSet<string> _validWindSpeedUnits = new HashSet<string>
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

            var forecast = _forecastsRepository.Get(parameters.Latitude, parameters.Longitude);
            if (forecast == null)
            {
                forecast = new Forecast
                {
                    Latitude = parameters.Latitude,
                    Longitude = parameters.Longitude,
                    LastUpdate = now,
                    Data = await GetFromProviderAsync(parameters)
                };

                await _forecastsRepository.CreateAsync(forecast);
            }
            else if (forecast.LastUpdate < now.AddMinutes(-30))
            {
                forecast.Data = await GetFromProviderAsync(parameters);
                await _forecastsRepository.UpdateAsync(forecast.Id, now, forecast.Data);
            }

            var data = JsonSerializer.Deserialize<OpenMeteoResult>(forecast.Data);

            var result = new ForecastResult
            {
                Temperature = ConvertTemperature(data.hourly.temperature_2m[parameters.Date.Hour], data.hourly_units.TemperatureUnitString, parameters.TemperatureUnit),
                Precipitation = ConvertPrecipitation(data.hourly.precipitation[parameters.Date.Hour], data.hourly_units.precipitation, parameters.PrecipitationUnit),
                WindSpeed = ConvertWindSpeed(data.hourly.windspeed_10m[parameters.Date.Hour], data.hourly_units.WindSpeedUnitString, parameters.WindSpeedUnit),
                WeatherCode = (WeatherCode)data.hourly.weathercode[parameters.Date.Hour]
            };

            int from = parameters.Date.Hour + 1;
            int to = from + 24;
            for (var i = from; i < to; i++)
            {
                result.Hourly.Add(new HourlyForecast(
                    Time: data.hourly.time[i].ToString("HH:mm"),
                    Temperature: ConvertTemperature(data.hourly.temperature_2m[i], data.hourly_units.TemperatureUnitString, parameters.TemperatureUnit),
                    Precipitation: ConvertPrecipitation(data.hourly.precipitation[i], data.hourly_units.precipitation, parameters.PrecipitationUnit),
                    WindSpeed: ConvertWindSpeed(data.hourly.windspeed_10m[i], data.hourly_units.WindSpeedUnitString, parameters.WindSpeedUnit),
                    WeatherCode: (WeatherCode)data.hourly.weathercode[i]
                ));
            }

            return result;
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
        queryString.Add("hourly", "temperature_2m,precipitation,windspeed_10m,weathercode");
        queryString.Add("timezone", "auto");

        using HttpClient httpClient = _httpClientFactory.CreateClient("open-meteo");
        HttpResponseMessage response = await httpClient.GetAsync("forecast?" + HttpUtility.UrlDecode(queryString.ToString()));

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }

    private short ConvertTemperature(float temperature, string fromUnit, string toUnit)
    {
        if (fromUnit == toUnit)
        {
            return (short)Math.Round(temperature);
        }

        if (fromUnit == "celsius")
        {
            return (short)Math.Round((temperature * 1.8) + 32);
        }

        return (short)Math.Round((temperature - 32) / 1.8);
    }

    private short ConvertPrecipitation(float precipitation, string fromUnit, string toUnit)
    {
        if (fromUnit == toUnit)
        {
            return (short)Math.Round(precipitation);
        }

        if (fromUnit == "mm")
        {
            return (short)Math.Round(precipitation / 25.4);
        }

        return (short)Math.Round(precipitation * 25.4);
    }

    private short ConvertWindSpeed(float windSpeed, string fromUnit, string toUnit)
    {
        if (fromUnit == toUnit)
        {
            return (short)Math.Round(windSpeed);
        }

        if (fromUnit == "kmh")
        {
            return (short)Math.Round(windSpeed / 1.609);
        }

        return (short)Math.Round(windSpeed * 1.609);
    }
}
