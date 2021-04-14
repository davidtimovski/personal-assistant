using System;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Persistence;
using PersonalAssistant.Application.Contracts.Accountant.Common;

namespace PersonalAssistant.Persistence.Repositories.Accountant
{
    public class DeletedEntitiesRepository : BaseRepository, IDeletedEntitiesRepository
    {
        public DeletedEntitiesRepository(PersonalAssistantContext efContext)
            : base(efContext) { }

        public async Task DeleteOldAsync(DateTime from)
        {
            using IDbConnection conn = OpenConnection();

            await conn.ExecuteAsync(@"DELETE FROM ""Accountant.DeletedEntities"" WHERE ""DeletedDate"" < @DeleteFrom", new { DeleteFrom = from });
        }
    }
}
