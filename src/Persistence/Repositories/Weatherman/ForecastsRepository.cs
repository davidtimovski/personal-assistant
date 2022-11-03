using System;
using System.Data;
using System.Threading.Tasks;
using Application.Contracts.Weatherman.Forecasts;
using Application.Contracts.Weatherman.Forecasts.Models;
using Dapper;
using Domain.Entities.Weatherman;

namespace Persistence.Repositories.Weatherman;

public class ForecastsRepository : BaseRepository, IForecastsRepository
{
    public ForecastsRepository(PersonalAssistantContext efContext)
        : base(efContext) { }

    public Forecast Get(GetForecast parameters)
    {
        using IDbConnection conn = OpenConnection();

        return conn.QueryFirstOrDefault<Forecast>(@"SELECT * FROM weatherman_forecasts 
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
