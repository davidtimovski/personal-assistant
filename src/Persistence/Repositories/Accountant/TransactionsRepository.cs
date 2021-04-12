using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Persistence;
using PersonalAssistant.Application.Contracts.Accountant.Transactions;
using PersonalAssistant.Domain.Entities.Accountant;

namespace PersonalAssistant.Persistence.Repositories.Accountant
{
    public class TransactionsRepository : BaseRepository, ITransactionsRepository
    {
        public TransactionsRepository(PersonalAssistantContext efContext)
            : base(efContext) { }

        public async Task<IEnumerable<Transaction>> GetAllForExportAsync(int userId, string uncategorized)
        {
            var sql = @"SELECT t.*, fa.""Id"", fa.""Name"", ta.""Id"", ta.""Name"", c.""Id"", c.""Name"", pc.""Id"", pc.""Name""
                        FROM ""Accountant.Transactions"" AS t
                        LEFT JOIN ""Accountant.Accounts"" AS fa ON t.""FromAccountId"" = fa.""Id""
                        LEFT JOIN ""Accountant.Accounts"" AS ta ON t.""ToAccountId"" = ta.""Id""
                        LEFT JOIN ""Accountant.Categories"" AS c ON t.""CategoryId"" = c.""Id""
                        LEFT JOIN ""Accountant.Categories"" AS pc ON c.""ParentId"" = pc.""Id""
                        WHERE fa.""UserId"" = @UserId OR ta.""UserId"" = @UserId ORDER BY ""Date""";

            var transactions = await Dapper.QueryAsync<Transaction, Account, Account, Category, Category, Transaction>(sql,
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
                }, new { UserId = userId }, null, true);

            return transactions;
        }

        public async Task<IEnumerable<Transaction>> GetAllAsync(int userId, DateTime fromModifiedDate)
        {
            return await Dapper.QueryAsync<Transaction>(@"SELECT t.* 
                                                        FROM ""Accountant.Transactions"" AS t
                                                        INNER JOIN ""Accountant.Accounts"" AS a ON a.""Id"" = t.""FromAccountId"" 
                                                            OR a.""Id"" = t.""ToAccountId"" 
                                                        WHERE a.""UserId"" = @UserId AND t.""ModifiedDate"" > @FromModifiedDate",
                new { UserId = userId, FromModifiedDate = fromModifiedDate });
        }

        public async Task<IEnumerable<Transaction>> GetAllAsync(int userId, int categoryId, DateTime from, DateTime to)
        {
            return await Dapper.QueryAsync<Transaction>(@"SELECT t.* 
                                                        FROM ""Accountant.Transactions"" AS t 
                                                        INNER JOIN ""Accountant.Accounts"" AS a ON a.""Id"" = t.""FromAccountId"" 
                                                            OR a.""Id"" = t.""ToAccountId"" 
                                                        WHERE a.""UserId"" = @UserId 
                                                            AND ""CategoryId"" = @CategoryId 
                                                            AND ""Date"" >= @From AND ""Date"" < @To 
                                                            AND ""FromAccountId"" IS NOT NULL AND ""ToAccountId"" IS NULL",
                                                        new { UserId = userId, CategoryId = categoryId, From = from, To = to });
        }

        public async Task<bool> AnyAsync(int userId, int categoryId, DateTime from)
        {
            return await Dapper.ExecuteScalarAsync<bool>(@"SELECT COUNT(*) 
                                                        FROM ""Accountant.Transactions"" AS t 
                                                        INNER JOIN ""Accountant.Accounts"" AS a ON a.""Id"" = t.""FromAccountId"" 
                                                            OR a.""Id"" = t.""ToAccountId"" 
                                                        WHERE a.""UserId"" = @UserId 
                                                            AND ""CategoryId"" = @CategoryId 
                                                            AND ""Date"" >= @From 
                                                            AND ""FromAccountId"" IS NOT NULL AND ""ToAccountId"" IS NULL",
                                                        new { UserId = userId, CategoryId = categoryId, From = from });
        }

        public async Task<IEnumerable<int>> GetDeletedIdsAsync(int userId, DateTime fromDate)
        {
            return await Dapper.QueryAsync<int>(@"SELECT ""EntityId"" FROM ""Accountant.DeletedEntities"" WHERE ""UserId"" = @UserId AND ""EntityType"" = @EntityType AND ""DeletedDate"" > @DeletedDate",
                new { UserId = userId, EntityType = (short)EntityType.Transaction, DeletedDate = fromDate });
        }

        public async Task<int> CreateAsync(Transaction transaction, DbConnection uowConn = null, DbTransaction uowTransaction = null)
        {
            int id;

            if (uowConn == null && uowTransaction == null)
            {
                var dbTransaction = Dapper.BeginTransaction();

                id = (await Dapper.QueryAsync<int>(@"INSERT INTO ""Accountant.Transactions"" 
                    (""FromAccountId"", ""ToAccountId"", ""CategoryId"", ""Amount"", ""FromStocks"", ""ToStocks"", ""Currency"", ""Description"", ""Date"", ""IsEncrypted"", ""EncryptedDescription"", ""Salt"", ""Nonce"", ""CreatedDate"", ""ModifiedDate"")
                    VALUES 
                    (@FromAccountId, @ToAccountId, @CategoryId, @Amount, @FromStocks, @ToStocks, @Currency, @Description, @Date, @IsEncrypted, @EncryptedDescription, @Salt, @Nonce, @CreatedDate, @ModifiedDate) returning ""Id""",
                    transaction, dbTransaction)).Single();

                if (transaction.Amount < 0)
                {
                    var relatedUpcomingExpenses = await Dapper.QueryAsync<UpcomingExpense>(@"SELECT * FROM ""Accountant.UpcomingExpenses"" WHERE ""CategoryId"" = @CategoryId",
                        new { transaction.CategoryId });

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
                                    await Dapper.ExecuteAsync(@"UPDATE ""Accountant.UpcomingExpenses"" SET ""Amount"" = ""Amount"" - @Amount WHERE ""Id"" = @Id",
                                        new { upcomingExpense.Id, transaction.Amount }, dbTransaction);
                                }
                                else
                                {
                                    await Dapper.ExecuteAsync(@"DELETE FROM ""Accountant.UpcomingExpenses"" WHERE ""Id"" = @Id",
                                        new { upcomingExpense.Id }, dbTransaction);

                                    await Dapper.QueryAsync<int>(@"INSERT INTO ""Accountant.DeletedEntities"" (""UserId"", ""EntityType"", ""EntityId"", ""DeletedDate"")
                                         VALUES (@UserId, @EntityType, @EntityId, @DeletedDate)",
                                         new { UserId = upcomingExpense.UserId, EntityType = (short)EntityType.UpcomingExpense, EntityId = upcomingExpense.Id, DeletedDate = DateTime.UtcNow },
                                         dbTransaction);
                                }
                            }
                        }
                    }
                }

                dbTransaction.Commit();
            }
            else
            {
                id = (await uowConn.QueryAsync<int>(@"INSERT INTO ""Accountant.Transactions"" 
                    (""FromAccountId"", ""ToAccountId"", ""CategoryId"", ""Amount"", ""FromStocks"", ""ToStocks"", ""Currency"", ""Description"", ""Date"", ""IsEncrypted"", ""EncryptedDescription"", ""Salt"", ""Nonce"", ""CreatedDate"", ""ModifiedDate"")
                    VALUES 
                    (@FromAccountId, @ToAccountId, @CategoryId, @Amount, @FromStocks, @ToStocks, @Currency, @Description, @Date, @IsEncrypted, @EncryptedDescription, @Salt, @Nonce, @CreatedDate, @ModifiedDate) returning ""Id""",
                    transaction, uowTransaction)).Single();
            }

            return id;
        }

        public async Task UpdateAsync(Transaction transaction)
        {
            await Dapper.ExecuteAsync(@"UPDATE ""Accountant.Transactions"" SET ""FromAccountId"" = @FromAccountId, ""ToAccountId"" = @ToAccountId, 
                                        ""CategoryId"" = @CategoryId, 
                                        ""Amount"" = @Amount, ""FromStocks"" = @FromStocks, ""ToStocks"" = @ToStocks, 
                                        ""Currency"" = @Currency, ""Description"" = @Description, ""Date"" = @Date, 
                                        ""IsEncrypted"" = @IsEncrypted, ""EncryptedDescription"" = @EncryptedDescription, ""Salt"" = @Salt,
                                        ""Nonce"" = @Nonce, ""ModifiedDate"" = @ModifiedDate WHERE ""Id"" = @Id",
                                        transaction);
        }

        public async Task DeleteAsync(int id, int userId)
        {
            var transaction = Dapper.BeginTransaction();

            var deletedEntryExists = await Dapper.ExecuteScalarAsync<bool>(@"SELECT COUNT(*)
                                                            FROM ""Accountant.DeletedEntities""
                                                            WHERE ""UserId"" = @UserId AND ""EntityType"" = @EntityType AND ""EntityId"" = @EntityId",
                                                            new { UserId = userId, EntityType = (short)EntityType.Transaction, EntityId = id });

            if (deletedEntryExists)
            {
                await Dapper.QueryAsync<int>(@"UPDATE ""Accountant.DeletedEntities"" SET ""DeletedDate"" = @DeletedDate
                                             WHERE ""UserId"" = @UserId AND ""EntityType"" = @EntityType AND ""EntityId"" = @EntityId",
                                             new { UserId = userId, EntityType = (short)EntityType.Transaction, EntityId = id, DeletedDate = DateTime.UtcNow },
                                             transaction);
            }
            else
            {
                await Dapper.QueryAsync<int>(@"INSERT INTO ""Accountant.DeletedEntities"" (""UserId"", ""EntityType"", ""EntityId"", ""DeletedDate"")
                                         VALUES (@UserId, @EntityType, @EntityId, @DeletedDate)",
                                         new { UserId = userId, EntityType = (short)EntityType.Transaction, EntityId = id, DeletedDate = DateTime.UtcNow },
                                         transaction);
            }

            await Dapper.ExecuteAsync(@"DELETE 
                                    FROM 
                                            ""Accountant.Transactions"" AS t 
                                    USING 
                                            ""Accountant.Accounts"" AS a
                                    WHERE 
  	                                    (a.""Id"" = t.""FromAccountId"" 
	                                    OR a.""Id"" = t.""ToAccountId"")
	                                    AND t.""Id"" = @Id AND a.""UserId"" = @UserId",
                new { Id = id, UserId = userId }, transaction);

            transaction.Commit();
        }
    }
}
