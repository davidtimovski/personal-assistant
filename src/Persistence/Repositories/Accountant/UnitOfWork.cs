using System.Data.Common;
using System.Threading.Tasks;
using Persistence;
using PersonalAssistant.Application.Contracts.Accountant.Common;

namespace PersonalAssistant.Persistence.Repositories.Accountant
{
    public class UnitOfWork : BaseRepository, IUnitOfWork
    {
        public UnitOfWork(PersonalAssistantContext efContext)
            : base(efContext) { }

        public (DbConnection conn, DbTransaction transaction) StartTransaction()
        {
            DbConnection conn = Dapper;

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
