using System.Data;
using Application.Domain.Accountant;
using Core.Application.Contracts;
using Dapper;

namespace Core.Persistence.Repositories;

public class CsvRepository : BaseRepository, ICsvRepository
{
    public CsvRepository(PersonalAssistantContext efContext)
        : base(efContext) { }

    public IEnumerable<Transaction> GetAllTransactionsForExport(int userId, string uncategorized)
    {
        using IDbConnection conn = OpenConnection();

        const string query = @"SELECT t.*, fa.id, fa.name, ta.id, ta.name, c.id, c.name, pc.id, pc.name
                        FROM accountant.transactions AS t
                        LEFT JOIN accountant.accounts AS fa ON t.from_account_id = fa.id
                        LEFT JOIN accountant.accounts AS ta ON t.to_account_id = ta.id
                        LEFT JOIN accountant.categories AS c ON t.category_id = c.id
                        LEFT JOIN accountant.categories AS pc ON c.parent_id = pc.id
                        WHERE fa.user_id = @UserId OR ta.user_id = @UserId ORDER BY date";

        var transactions = conn.Query<Transaction, Account, Account, Category, Category, Transaction>(query,
            (transaction, fromAccount, toAccount, category, parentCategory) =>
            {
                transaction.FromAccount = fromAccount ?? new Account();
                transaction.ToAccount = toAccount ?? new Account();

                if (category == null)
                {
                    transaction.Category = new Category { Name = uncategorized };
                }
                else
                {
                    if (parentCategory != null)
                    {
                        category.Name = $"{parentCategory.Name}/{category.Name}";
                    }

                    transaction.Category = category;
                }

                return transaction;
            }, new { UserId = userId });

        return transactions;
    }
}
