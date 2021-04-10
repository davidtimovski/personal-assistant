using System;
using System.Data.Common;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Options;
using Persistence;
using PersonalAssistant.Application.Contracts.Common;
using PersonalAssistant.Domain.Entities.Common;

namespace PersonalAssistant.Persistence.Repositories.Common
{
    public class CurrencyRatesRepository : BaseRepository, ICurrencyRatesRepository
    {
        public CurrencyRatesRepository(IOptions<DatabaseSettings> databaseSettings, PersonalAssistantContext efContext)
            : base(databaseSettings.Value.DefaultConnectionString, efContext) { }

        public async Task<CurrencyRates> GetAsync(DateTime date)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            var rates = await conn.QueryFirstOrDefaultAsync<CurrencyRates>(@"SELECT * FROM ""CurrencyRates"" WHERE ""Date"" = @Date", new { Date = date });
            if (rates != null)
            {
                return rates;
            }

            rates = await conn.QueryFirstOrDefaultAsync<CurrencyRates>(@"SELECT * FROM ""CurrencyRates"" WHERE ""Date"" < @Date ORDER BY ""Date"" DESC LIMIT 1", new { Date = date });
            if (rates != null)
            {
                return rates;
            }

            return await conn.QueryFirstOrDefaultAsync<CurrencyRates>(@"SELECT * FROM ""CurrencyRates"" WHERE ""Date"" > @Date ORDER BY ""Date"" DESC LIMIT 1",
                new { Date = date });
        }

        public async Task CreateAsync(CurrencyRates rates)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            var exists = await conn.ExecuteScalarAsync<bool>(@"SELECT COUNT(*) FROM ""CurrencyRates"" WHERE ""Date"" = @Date", new { rates.Date });
            if (exists)
            {
                return;
            }

            await conn.ExecuteAsync(@"INSERT INTO ""CurrencyRates"" (""Date"", ""Rates"") VALUES (@Date, CAST(@Rates AS json))", rates);
        }
    }
}
