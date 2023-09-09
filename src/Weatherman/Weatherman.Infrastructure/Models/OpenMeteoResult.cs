namespace Weatherman.Infrastructure.Models;

internal class OpenMeteoResult
{
    public required OpenMeteoHourly hourly { get; set; }
    public required OpenMeteoDailyUnits daily_units { get; set; }
    public required OpenMeteoDaily daily { get; set; }
}

internal class OpenMeteoDailyUnits
{
    public required string temperature_2m_max { get; set; }
    public required string temperature_2m_min { get; set; }
}

internal class OpenMeteoHourly
{
    public required DateTime[] time { get; set; }
    public required float[] temperature_2m { get; set; }
    public required float[] apparent_temperature { get; set; }
    public required float[] precipitation { get; set; }
    public required float[] windspeed_10m { get; set; }
    public required short[] weathercode { get; set; }
}

internal class OpenMeteoDaily
{
    public required DateTime[] time { get; set; }
    public required short[] weathercode { get; set; }
    public required float[] temperature_2m_max { get; set; }
    public required float[] temperature_2m_min { get; set; }
    public required float[] precipitation_sum { get; set; }
    public required DateTime[] sunrise { get; set; }
    public required DateTime[] sunset { get; set; }
}
