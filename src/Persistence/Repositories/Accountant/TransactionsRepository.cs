using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Application.Contracts.Accountant.Transactions;
using Dapper;
using Domain.Entities.Accountant;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Repositories.Accountant;

public class TransactionsRepository : BaseRepository, ITransactionsRepository
{
    public TransactionsRepository(PersonalAssistantContext efContext)
        : base(efContext) { }

    public IEnumerable<Transaction> GetAllForExport(int userId, string uncategorized)
    {
        using IDbConnection conn = OpenConnection();

        const string query = @"SELECT t.*, fa.id, fa.name, ta.id, ta.name, c.id, c.name, pc.id, pc.name
                        FROM accountant_transactions AS t
                        LEFT JOIN accountant_accounts AS fa ON t.from_account_id = fa.id
                        LEFT JOIN accountant_accounts AS ta ON t.to_account_id = ta.id
                        LEFT JOIN accountant_categories AS c ON t.category_id = c.id
                        LEFT JOIN accountant_categories AS pc ON c.parent_id = pc.id
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

    public IEnumerable<Transaction> GetAll(int userId, DateTime fromModifiedDate)
    {
        using IDbConnection conn = OpenConnection();

        return conn.Query<Transaction>(@"SELECT t.* 
                                         FROM accountant_transactions AS t
                                         INNER JOIN accountant_accounts AS a ON a.id = t.from_account_id 
                                             OR a.id = t.to_account_id 
                                         WHERE a.user_id = @UserId AND t.modified_date > @FromModifiedDate",
            new { UserId = userId, FromModifiedDate = fromModifiedDate });
    }

    public IEnumerable<int> GetDeletedIds(int userId, DateTime fromDate)
    {
        using IDbConnection conn = OpenConnection();

        return conn.Query<int>(@"SELECT entity_id FROM accountant_deleted_entities WHERE user_id = @UserId AND entity_type = @EntityType AND deleted_date > @DeletedDate",
            new { UserId = userId, EntityType = (short)EntityType.Transaction, DeletedDate = fromDate });
    }

    public async Task<int> CreateAsync(Transaction transaction)
    {
        using IDbConnection conn = OpenConnection();

        int id;
        var insertQuery = @"INSERT INTO accountant_transactions 
                                (from_account_id, to_account_id, category_id, amount, from_stocks, to_stocks, currency, description, date, is_encrypted, encrypted_description, salt, nonce, created_date, modified_date)
                                VALUES 
                                (@FromAccountId, @ToAccountId, @CategoryId, @Amount, @FromStocks, @ToStocks, @Currency, @Description, @Date, @IsEncrypted, @EncryptedDescription, @Salt, @Nonce, @CreatedDate, @ModifiedDate) returning id";

        var dbTransaction = conn.BeginTransaction();

        id = (await conn.QueryAsync<int>(insertQuery, transaction, dbTransaction)).Single();

        if (transaction.FromAccountId.HasValue && !transaction.ToAccountId.HasValue)
        {
            var relatedUpcomingExpenses = conn.Query<UpcomingExpense>(@"SELECT * FROM accountant_upcoming_expenses
                                                                        WHERE category_id = @CategoryId 
                                                                            AND EXTRACT(year FROM date) = @Year
                                                                            AND EXTRACT(month FROM date) = @Month",
                new { transaction.CategoryId, transaction.Date.Year, transaction.Date.Month }).ToList();

            if (relatedUpcomingExpenses.Any())
            {
                bool transactionHasDescription = !string.IsNullOrEmpty(transaction.Description);

                foreach (var upcomingExpense in relatedUpcomingExpenses)
                {
                    bool upcomingExpenseHasDescription = !string.IsNullOrEmpty(upcomingExpense.Description);
                    bool bothWithDescriptionsAndTheyMatch = upcomingExpenseHasDescription
                                                            && transactionHasDescription
                                                            && string.Equals(upcomingExpense.Description, transaction.Description.Trim(), StringComparison.OrdinalIgnoreCase);

                    if (!upcomingExpenseHasDescription || bothWithDescriptionsAndTheyMatch)
                    {
                        if (upcomingExpense.Amount > transaction.Amount)
                        {
                            await conn.ExecuteAsync(@"UPDATE accountant_upcoming_expenses SET amount = amount - @Amount, modified_date = @ModifiedDate WHERE id = @Id",
                                new { upcomingExpense.Id, transaction.Amount, ModifiedDate = DateTime.UtcNow }, dbTransaction);
                        }
                        else
                        {
                            await conn.ExecuteAsync(@"DELETE FROM accountant_upcoming_expenses WHERE id = @Id",
                                new { upcomingExpense.Id }, dbTransaction);

                            await conn.QueryAsync<int>(@"INSERT INTO accountant_deleted_entities (user_id, entity_type, entity_id, deleted_date)
                                                         VALUES (@UserId, @EntityType, @EntityId, @DeletedDate)",
                                new { upcomingExpense.UserId, EntityType = (short)EntityType.UpcomingExpense, EntityId = upcomingExpense.Id, DeletedDate = DateTime.UtcNow },
                                dbTransaction);
                        }
                    }
                }
            }
        }

        dbTransaction.Commit();

        return id;
    }

    public async Task UpdateAsync(Transaction transaction)
    {
        Transaction dbTransaction = EFContext.Transactions.Find(transaction.Id);

        dbTransaction.FromAccountId = transaction.FromAccountId;
        dbTransaction.ToAccountId = transaction.ToAccountId;
        dbTransaction.CategoryId = transaction.CategoryId;
        dbTransaction.Amount = transaction.Amount;
        dbTransaction.FromStocks = transaction.FromStocks;
        dbTransaction.ToStocks = transaction.ToStocks;
        dbTransaction.Currency = transaction.Currency;
        dbTransaction.Description = transaction.Description;
        dbTransaction.Date = transaction.Date.ToUniversalTime();
        dbTransaction.IsEncrypted = transaction.IsEncrypted;
        dbTransaction.EncryptedDescription = transaction.EncryptedDescription;
        dbTransaction.Salt = transaction.Salt;
        dbTransaction.Nonce = transaction.Nonce;
        dbTransaction.ModifiedDate = transaction.ModifiedDate;

        await EFContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id, int userId)
    {
        var deletedEntity = EFContext.DeletedEntities.FirstOrDefault(x => x.UserId == userId && x.EntityType == EntityType.Transaction && x.EntityId == id);
        if (deletedEntity == null)
        {
            EFContext.DeletedEntities.Add(new DeletedEntity
            {
                UserId = userId,
                EntityType = EntityType.Transaction,
                EntityId = id,
                DeletedDate = DateTime.UtcNow
            });
        }
        else
        {
            deletedEntity.DeletedDate = DateTime.UtcNow;
        }

        var transaction = EFContext.Transactions.First(x => x.Id == id 
            && !x.FromAccountId.HasValue || x.FromAccount.UserId == userId
            && !x.ToAccountId.HasValue || x.ToAccount.UserId == userId);

        EFContext.Transactions.Remove(transaction);

        await EFContext.SaveChangesAsync();
    }
}
