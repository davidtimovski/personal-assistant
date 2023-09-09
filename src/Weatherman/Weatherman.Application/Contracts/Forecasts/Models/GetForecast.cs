namespace Weatherman.Application.Contracts.Forecasts.Models;

public class GetForecast
{
    public required float Latitude { get; set; }
    public required float Longitude { get; set; }
    public required string TemperatureUnit { get; set; }
    public required string PrecipitationUnit { get; set; }
    public required string WindSpeedUnit { get; set; }
    public required DateTime Time { get; set; }
}
