using System.Data;
using Application.Contracts.Accountant.Common;

namespace Persistence.Repositories.Accountant;

public class UnitOfWork : BaseRepository, IUnitOfWork
{
    public UnitOfWork(PersonalAssistantContext efContext)
        : base(efContext) { }

    public (IDbConnection conn, IDbTransaction transaction) StartTransaction()
    {
        IDbConnection conn = OpenConnection();
        var transaction = conn.BeginTransaction();

        return (conn, transaction);
    }

    public void CommitTransaction(IDbConnection conn, IDbTransaction transaction)
    {
        transaction.Commit();
        conn.Dispose();
    }
}