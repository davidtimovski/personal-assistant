using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Persistence;
using PersonalAssistant.Application.Contracts.Accountant.Common;

namespace PersonalAssistant.Persistence.Repositories.Accountant
{
    public class UnitOfWork : BaseRepository, IUnitOfWork
    {
        public UnitOfWork(IOptions<DatabaseSettings> databaseSettings, PersonalAssistantContext efContext)
            : base(databaseSettings.Value.DefaultConnectionString, efContext) { }

        public async Task<(DbConnection conn, DbTransaction transaction)> StartTransactionAsync()
        {
            DbConnection conn = Connection;

            await conn.OpenAsync();
            var transaction = conn.BeginTransaction();

            return (conn, transaction);
        }

        public async Task CommitTransactionAsync(DbConnection conn, DbTransaction transaction)
        {
            await transaction.CommitAsync();
            await conn.DisposeAsync();
        }
    }
}
