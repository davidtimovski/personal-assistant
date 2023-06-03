namespace Weatherman.Application.Entities;

public class Forecast
{
    public int Id { get; set; }
    public float Latitude { get; set; }
    public float Longitude { get; set; }
    public string TemperatureUnit { get; set; } = null!;
    public string PrecipitationUnit { get; set; } = null!;
    public string WindSpeedUnit { get; set; } = null!;
    public DateTime LastUpdate { get; set; }
    public string Data { get; set; } = null!;
}
