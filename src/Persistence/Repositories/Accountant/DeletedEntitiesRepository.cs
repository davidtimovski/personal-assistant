using System;
using System.Data.Common;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Options;
using Persistence;
using PersonalAssistant.Application.Contracts.Accountant.Common;

namespace PersonalAssistant.Persistence.Repositories.Accountant
{
    public class DeletedEntitiesRepository : BaseRepository, IDeletedEntitiesRepository
    {
        public DeletedEntitiesRepository(IOptions<DatabaseSettings> databaseSettings, PersonalAssistantContext efContext)
            : base(databaseSettings.Value.DefaultConnectionString, efContext) { }

        public async Task DeleteOldAsync(DateTime from)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            await conn.ExecuteAsync(@"DELETE FROM ""Accountant.DeletedEntities"" WHERE ""DeletedDate"" < @DeleteFrom", new { DeleteFrom = from });
        }
    }
}
