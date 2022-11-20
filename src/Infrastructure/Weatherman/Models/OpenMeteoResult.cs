namespace Infrastructure.Weatherman.Models;

internal class OpenMeteoResult
{
    public OpenMeteoHourly hourly { get; set; }
    public OpenMeteoDailyUnits daily_units { get; set; }
    public OpenMeteoDaily daily { get; set; }
}

internal class OpenMeteoDailyUnits
{
    public string temperature_2m_max { get; set; }
    public string temperature_2m_min { get; set; }
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
    public DateTime[] time { get; set; }
    public short[] weathercode { get; set; }
    public float[] temperature_2m_max { get; set; }
    public float[] temperature_2m_min { get; set; }
    public float[] precipitation_sum { get; set; }
    public DateTime[] sunrise { get; set; }
    public DateTime[] sunset { get; set; }
}
