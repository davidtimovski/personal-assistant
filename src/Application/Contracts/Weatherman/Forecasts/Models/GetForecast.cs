using System;

namespace Application.Contracts.Weatherman.Forecasts.Models;

public class GetForecast
{
    public float Latitude { get; set; }
    public float Longitude { get; set; }
    public string TemperatureUnit { get; set; }
    public string PrecipitationUnit { get; set; }
    public string WindSpeedUnit { get; set; }
    public DateTime Time { get; set; }
}
