using System.Data;
using Dapper;
using Domain.Weatherman;
using Persistence;
using Weatherman.Application.Contracts.Forecasts;
using Weatherman.Application.Contracts.Forecasts.Models;

namespace Weatherman.Persistence.Repositories;

public class ForecastsRepository : BaseRepository, IForecastsRepository
{
    public ForecastsRepository(PersonalAssistantContext efContext)
        : base(efContext) { }

    public Forecast Get(GetForecast parameters)
    {
        using IDbConnection conn = OpenConnection();

        return conn.QueryFirstOrDefault<Forecast>(@"SELECT * FROM weatherman.forecasts 
            WHERE latitude = @Latitude AND longitude = @Longitude 
                AND temperature_unit = @TemperatureUnit AND precipitation_unit = @PrecipitationUnit AND wind_speed_unit = @WindSpeedUnit", parameters);
    }

    public async Task CreateAsync(Forecast model)
    {
        EFContext.Forecasts.Add(model);
        await EFContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(int id, DateTime lastUpdate, string data)
    {
        Forecast dbForecast = EFContext.Forecasts.Find(id);
        dbForecast.LastUpdate = lastUpdate;
        dbForecast.Data = data;
        await EFContext.SaveChangesAsync();
    }
}
