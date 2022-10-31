using System;

namespace Infrastructure.Weatherman.Models;

internal class OpenMeteoResult
{
    public OpenMeteoHourlyUnits hourly_units { get; set; }
    public OpenMeteoHourly hourly { get; set; }
    public OpenMeteoDailyUnits daily_units { get; set; }
    public OpenMeteoDaily daily { get; set; }
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

internal class OpenMeteoDailyUnits
{
    public string temperature_2m_max { get; set; }
    public string temperature_2m_min { get; set; }

    /// <summary>
    /// Because the query paramter is formatted like "celsius" but the daily_units value is like "°C".
    /// </summary>
    public string TemperatureMaxUnitString
    {
        get
        {
            return temperature_2m_max == "°C" ? "celsius" : "fahrenheit";
        }
    }

    /// <summary>
    /// Because the query paramter is formatted like "celsius" but the daily_units value is like "°C".
    /// </summary>
    public string TemperatureMinUnitString
    {
        get
        {
            return temperature_2m_min == "°C" ? "celsius" : "fahrenheit";
        }
    }
}

internal class OpenMeteoHourly
{
    public DateTime[] time { get; set; }
    public float[] temperature_2m { get; set; }
    public float[] apparent_temperature { get; set; }
    public float[] precipitation { get; set; }
    public float[] windspeed_10m { get; set; }
    public short[] weathercode { get; set; }
}

internal class OpenMeteoDaily
{
    public short[] weathercode { get; set; }
    public float[] temperature_2m_max { get; set; }
    public float[] temperature_2m_min { get; set; }
    public DateTime[] sunrise { get; set; }
    public DateTime[] sunset { get; set; }
}
