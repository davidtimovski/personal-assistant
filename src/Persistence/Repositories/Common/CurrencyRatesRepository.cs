using System;
using System.Threading.Tasks;
using Dapper;
using Persistence;
using PersonalAssistant.Application.Contracts.Common;
using PersonalAssistant.Domain.Entities.Common;

namespace PersonalAssistant.Persistence.Repositories.Common
{
    public class CurrencyRatesRepository : BaseRepository, ICurrencyRatesRepository
    {
        public CurrencyRatesRepository(PersonalAssistantContext efContext)
            : base(efContext) { }

        public async Task<CurrencyRates> GetAsync(DateTime date)
        {
            var rates = await Dapper.QueryFirstOrDefaultAsync<CurrencyRates>(@"SELECT * FROM ""CurrencyRates"" WHERE ""Date"" = @Date", new { Date = date });
            if (rates != null)
            {
                return rates;
            }

            rates = await Dapper.QueryFirstOrDefaultAsync<CurrencyRates>(@"SELECT * FROM ""CurrencyRates"" WHERE ""Date"" < @Date ORDER BY ""Date"" DESC LIMIT 1", new { Date = date });
            if (rates != null)
            {
                return rates;
            }

            return await Dapper.QueryFirstOrDefaultAsync<CurrencyRates>(@"SELECT * FROM ""CurrencyRates"" WHERE ""Date"" > @Date ORDER BY ""Date"" DESC LIMIT 1",
                new { Date = date });
        }

        public async Task CreateAsync(CurrencyRates rates)
        {
            var exists = await Dapper.ExecuteScalarAsync<bool>(@"SELECT COUNT(*) FROM ""CurrencyRates"" WHERE ""Date"" = @Date", new { rates.Date });
            if (exists)
            {
                return;
            }

            await Dapper.ExecuteAsync(@"INSERT INTO ""CurrencyRates"" (""Date"", ""Rates"") VALUES (@Date, CAST(@Rates AS json))", rates);
        }
    }
}
