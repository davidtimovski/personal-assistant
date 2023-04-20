using System.Data;
using Accountant.Application.Contracts.Transactions;
using Application.Domain.Accountant;
using Core.Persistence;
using Dapper;

namespace Persistence.Repositories.Accountant;

public class TransactionsRepository : BaseRepository, ITransactionsRepository
{
    public TransactionsRepository(PersonalAssistantContext efContext)
        : base(efContext) { }

    public IEnumerable<Transaction> GetAllForExport(int userId, string uncategorized)
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
