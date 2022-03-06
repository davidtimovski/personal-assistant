using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace Application.Contracts.Accountant.Common;

public interface IUnitOfWork
{
    (IDbConnection conn, IDbTransaction transaction) StartTransaction();
    void CommitTransaction(IDbConnection conn, IDbTransaction transaction);
}
