using System;
using System.Data;
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
            using IDbConnection conn = OpenConnection();

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
    }
}
