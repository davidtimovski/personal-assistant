using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Application.Contracts.Accountant.Transactions;
using Domain.Entities.Accountant;

namespace Persistence.Repositories.Accountant
{
    public class TransactionsRepository : BaseRepository, ITransactionsRepository
    {
        public TransactionsRepository(PersonalAssistantContext efContext)
            : base(efContext) { }

        public IEnumerable<Transaction> GetAllForExport(int userId, string uncategorized)
        {
            using IDbConnection conn = OpenConnection();

            const string query = @"SELECT t.*, fa.""Id"", fa.""Name"", ta.""Id"", ta.""Name"", c.""Id"", c.""Name"", pc.""Id"", pc.""Name""
                        FROM ""Accountant.Transactions"" AS t
                        LEFT JOIN ""Accountant.Accounts"" AS fa ON t.""FromAccountId"" = fa.""Id""
                        LEFT JOIN ""Accountant.Accounts"" AS ta ON t.""ToAccountId"" = ta.""Id""
                        LEFT JOIN ""Accountant.Categories"" AS c ON t.""CategoryId"" = c.""Id""
                        LEFT JOIN ""Accountant.Categories"" AS pc ON c.""ParentId"" = pc.""Id""
                        WHERE fa.""UserId"" = @UserId OR ta.""UserId"" = @UserId ORDER BY ""Date""";

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
                                            FROM ""Accountant.Transactions"" AS t
                                            INNER JOIN ""Accountant.Accounts"" AS a ON a.""Id"" = t.""FromAccountId"" 
                                                OR a.""Id"" = t.""ToAccountId"" 
                                            WHERE a.""UserId"" = @UserId AND t.""ModifiedDate"" > @FromModifiedDate",
                new { UserId = userId, FromModifiedDate = fromModifiedDate });
        }

        public IEnumerable<int> GetDeletedIds(int userId, DateTime fromDate)
        {
            using IDbConnection conn = OpenConnection();

            return conn.Query<int>(@"SELECT ""EntityId"" FROM ""Accountant.DeletedEntities"" WHERE ""UserId"" = @UserId AND ""EntityType"" = @EntityType AND ""DeletedDate"" > @DeletedDate",
                new { UserId = userId, EntityType = (short)EntityType.Transaction, DeletedDate = fromDate });
        }

        public async Task<int> CreateAsync(Transaction transaction, IDbConnection uowConn = null, IDbTransaction uowTransaction = null)
        {
            using IDbConnection conn = OpenConnection();

            int id;
            var insertQuery = @"INSERT INTO ""Accountant.Transactions"" 
                                (""FromAccountId"", ""ToAccountId"", ""CategoryId"", ""Amount"", ""FromStocks"", ""ToStocks"", ""Currency"", ""Description"", ""Date"", ""IsEncrypted"", ""EncryptedDescription"", ""Salt"", ""Nonce"", ""CreatedDate"", ""ModifiedDate"")
                                VALUES 
                                (@FromAccountId, @ToAccountId, @CategoryId, @Amount, @FromStocks, @ToStocks, @Currency, @Description, @Date, @IsEncrypted, @EncryptedDescription, @Salt, @Nonce, @CreatedDate, @ModifiedDate) returning ""Id""";

            if (uowConn == null && uowTransaction == null)
            {
                var dbTransaction = conn.BeginTransaction();

                id = (await conn.QueryAsync<int>(insertQuery, transaction, dbTransaction)).Single();

                if (transaction.FromAccountId.HasValue && !transaction.ToAccountId.HasValue)
                {
                    var relatedUpcomingExpenses = conn.Query<UpcomingExpense>(@"SELECT * FROM ""Accountant.UpcomingExpenses""
                                                                                WHERE ""CategoryId"" = @CategoryId 
                                                                                    AND EXTRACT(year FROM ""Date"") = @Year
                                                                                    AND EXTRACT(month FROM ""Date"") = @Month",
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
                                    await conn.ExecuteAsync(@"UPDATE ""Accountant.UpcomingExpenses"" SET ""Amount"" = ""Amount"" - @Amount, ""ModifiedDate"" = @ModifiedDate WHERE ""Id"" = @Id",
                                        new { upcomingExpense.Id, transaction.Amount, ModifiedDate = DateTime.UtcNow }, dbTransaction);
                                }
                                else
                                {
                                    await conn.ExecuteAsync(@"DELETE FROM ""Accountant.UpcomingExpenses"" WHERE ""Id"" = @Id",
                                        new { upcomingExpense.Id }, dbTransaction);

                                    await conn.QueryAsync<int>(@"INSERT INTO ""Accountant.DeletedEntities"" (""UserId"", ""EntityType"", ""EntityId"", ""DeletedDate"")
                                         VALUES (@UserId, @EntityType, @EntityId, @DeletedDate)",
                                         new { upcomingExpense.UserId, EntityType = (short)EntityType.UpcomingExpense, EntityId = upcomingExpense.Id, DeletedDate = DateTime.UtcNow },
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
                id = (await uowConn.QueryAsync<int>(insertQuery, transaction, uowTransaction)).Single();
            }

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
            using IDbConnection conn = OpenConnection();
            var transaction = conn.BeginTransaction();

            var deletedEntryExists = await conn.ExecuteScalarAsync<bool>(@"SELECT COUNT(*)
                                                            FROM ""Accountant.DeletedEntities""
                                                            WHERE ""UserId"" = @UserId AND ""EntityType"" = @EntityType AND ""EntityId"" = @EntityId",
                                                            new { UserId = userId, EntityType = (short)EntityType.Transaction, EntityId = id });

            if (deletedEntryExists)
            {
                await conn.QueryAsync<int>(@"UPDATE ""Accountant.DeletedEntities"" SET ""DeletedDate"" = @DeletedDate
                                             WHERE ""UserId"" = @UserId AND ""EntityType"" = @EntityType AND ""EntityId"" = @EntityId",
                                             new { UserId = userId, EntityType = (short)EntityType.Transaction, EntityId = id, DeletedDate = DateTime.UtcNow },
                                             transaction);
            }
            else
            {
                await conn.QueryAsync<int>(@"INSERT INTO ""Accountant.DeletedEntities"" (""UserId"", ""EntityType"", ""EntityId"", ""DeletedDate"")
                                         VALUES (@UserId, @EntityType, @EntityId, @DeletedDate)",
                                         new { UserId = userId, EntityType = (short)EntityType.Transaction, EntityId = id, DeletedDate = DateTime.UtcNow },
                                         transaction);
            }

            await conn.ExecuteAsync(@"DELETE 
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
