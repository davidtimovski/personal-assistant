using System;

namespace Infrastructure.Weatherman.Models;

internal class OpenMeteoResult
{
    public OpenMeteoHourlyUnits hourly_units { get; set; }
    public OpenMeteoHourly hourly { get; set; }
}

internal class OpenMeteoHourlyUnits
{
    public string temperature_2m { get; set; }
    public string precipitation { get; set; }
    public string windspeed_10m { get; set; }

    /// <summary>
    /// Because the query paramter is formatted like "celsius" but the hourly_units value is like "°C".
    /// </summary>
    public string TemperatureUnitString
    {
        get
        {
            return temperature_2m == "°C" ? "celsius" : "fahrenheit";
        }
    }

    /// <summary>
    /// Because the query paramter is formatted like "kmh" but the hourly_units value is like "km/h".
    /// </summary>
    public string WindSpeedUnitString
    {
        get
        {
            return windspeed_10m == "km/h" ? "kmh" : "mph";
        }
    }
}

internal class OpenMeteoHourly
{
    public DateTime[] time { get; set; }
    public float[] temperature_2m { get; set; }
    public float[] precipitation { get; set; }
    public float[] windspeed_10m { get; set; }
    public short[] weathercode { get; set; }
}
