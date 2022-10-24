using System;

namespace Domain.Entities.Weatherman;

public class Forecast
{
    public int Id { get; set; }
    public float Latitude { get; set; }
    public float Longitude { get; set; }
    public DateTime LastUpdate { get; set; }
    public string Data { get; set; }
}
