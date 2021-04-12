using System.Data.Common;
using System.Threading.Tasks;

namespace PersonalAssistant.Application.Contracts.Accountant.Common
{
    public interface IUnitOfWork
    {
        (DbConnection conn, DbTransaction transaction) StartTransaction();
        Task CommitTransactionAsync(DbConnection conn, DbTransaction transaction);
    }
}
