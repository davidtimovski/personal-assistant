using System;
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
            await Dapper.ExecuteAsync(@"DELETE FROM ""Accountant.DeletedEntities"" WHERE ""DeletedDate"" < @DeleteFrom", new { DeleteFrom = from });
        }
    }
}
