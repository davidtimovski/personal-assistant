using System.Data;
using Core.Persistence;
using Dapper;
using Sentry;
using Weatherman.Application.Contracts.Forecasts;
using Weatherman.Application.Contracts.Forecasts.Models;
using Weatherman.Application.Entities;

namespace Weatherman.Persistence.Repositories;

public class ForecastsRepository : BaseRepository, IForecastsRepository
{
    public ForecastsRepository(WeathermanContext efContext)
        : base(efContext) { }

    public Forecast Get(GetForecast parameters, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(ForecastsRepository)}.{nameof(Get)}");

        using IDbConnection conn = OpenConnection();

        var result = conn.QueryFirstOrDefault<Forecast>(@"SELECT * FROM weatherman.forecasts 
            WHERE latitude = @Latitude AND longitude = @Longitude 
                AND temperature_unit = @TemperatureUnit AND precipitation_unit = @PrecipitationUnit AND wind_speed_unit = @WindSpeedUnit", parameters);

        metric.Finish();

        return result;
    }

    public async Task CreateAsync(Forecast model, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(ForecastsRepository)}.{nameof(CreateAsync)}");

        EFContext.Forecasts.Add(model);
        await EFContext.SaveChangesAsync();

        metric.Finish();
    }

    public async Task UpdateAsync(int id, DateTime lastUpdate, string data, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(ForecastsRepository)}.{nameof(UpdateAsync)}");

        Forecast dbForecast = EFContext.Forecasts.First(x => x.Id == id);

        dbForecast.LastUpdate = lastUpdate;
        dbForecast.Data = data;
        await EFContext.SaveChangesAsync();

        metric.Finish();
    }
}
