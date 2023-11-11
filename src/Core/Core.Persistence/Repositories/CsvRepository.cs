using System.Data;
using Core.Application.Contracts;
using Core.Application.Contracts.Models;
using Dapper;
using Sentry;

namespace Core.Persistence.Repositories;

public class CsvRepository : BaseRepository, ICsvRepository
{
    public CsvRepository(CoreContext efContext)
        : base(efContext) { }

    public List<TransactionForExport> GetAllTransactionsForExport(int userId, string uncategorized, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(CsvRepository)}.{nameof(GetAllTransactionsForExport)}");

        try
        {
            using IDbConnection conn = OpenConnection();

            const string query = @"SELECT t.*, fa.id, fa.name, ta.id, ta.name, c.id, c.name, pc.id, pc.name
                        FROM accountant.transactions AS t
                        LEFT JOIN accountant.accounts AS fa ON t.from_account_id = fa.id
                        LEFT JOIN accountant.accounts AS ta ON t.to_account_id = ta.id
                        LEFT JOIN accountant.categories AS c ON t.category_id = c.id
                        LEFT JOIN accountant.categories AS pc ON c.parent_id = pc.id
                        WHERE fa.user_id = @UserId OR ta.user_id = @UserId ORDER BY date";

            var transactions = conn.Query<TransactionForExport, AccountForExport, AccountForExport, CategoryForExport, CategoryForExport, TransactionForExport>(query,
                (transaction, fromAccount, toAccount, category, parentCategory) =>
                {
                    transaction.FromAccount = fromAccount ?? new AccountForExport();
                    transaction.ToAccount = toAccount ?? new AccountForExport();

                    if (category is null)
                    {
                        transaction.Category = new CategoryForExport { Name = uncategorized };
                    }
                    else
                    {
                        if (parentCategory is not null)
                        {
                            category.Name = $"{parentCategory.Name}/{category.Name}";
                        }

                        transaction.Category = category;
                    }

                    return transaction;
                }, new { UserId = userId }).ToList();

            return transactions;
        }
        finally
        {
            metric.Finish();
        }
    }
}
