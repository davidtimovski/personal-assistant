namespace Application.Domain.Weatherman;

public class Forecast
{
    public int Id { get; set; }
    public float Latitude { get; set; }
    public float Longitude { get; set; }
    public string TemperatureUnit { get; set; }
    public string PrecipitationUnit { get; set; }
    public string WindSpeedUnit { get; set; }
    public DateTime LastUpdate { get; set; }
    public string Data { get; set; }
}
