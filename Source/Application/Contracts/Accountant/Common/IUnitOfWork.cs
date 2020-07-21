using System.Data.Common;
using System.Threading.Tasks;

namespace PersonalAssistant.Application.Contracts.Accountant.Common
{
    public interface IUnitOfWork
    {
        Task<(DbConnection conn, DbTransaction transaction)> StartTransactionAsync();
        Task CommitTransactionAsync(DbConnection conn, DbTransaction transaction);
    }
}
