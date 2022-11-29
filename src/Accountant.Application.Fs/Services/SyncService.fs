namespace Accountant.Application.Fs.Services

open Domain.Accountant
open Accountant.Application.Fs.Models.Sync

module SyncService =
    let mapEntitiesForSync (userId: int) (model: SyncEntities) =
        let accounts =
            model.Accounts
            |> Seq.map (fun x ->
                Account(
                    Id = x.Id,
                    UserId = userId,
                    Name = x.Name,
                    IsMain = x.IsMain,
                    Currency = x.Currency,
                    StockPrice = x.StockPrice,
                    CreatedDate = x.CreatedDate,
                    ModifiedDate = x.ModifiedDate
                ))

        let categories =
            model.Categories
            |> Seq.map (fun x ->
                Category(
                    Id = x.Id,
                    UserId = userId,
                    ParentId = x.ParentId,
                    Name = x.Name,
                    Type = x.Type,
                    GenerateUpcomingExpense = x.GenerateUpcomingExpense,
                    IsTax = x.IsTax,
                    CreatedDate = x.CreatedDate,
                    ModifiedDate = x.ModifiedDate
                ))

        let transactions =
            model.Transactions
            |> Seq.map (fun x ->
                Transaction(
                    Id = x.Id,
                    FromAccountId = x.FromAccountId,
                    ToAccountId = x.ToAccountId,
                    CategoryId = x.CategoryId,
                    Amount = x.Amount,
                    FromStocks = x.FromStocks,
                    ToStocks = x.ToStocks,
                    Currency = x.Currency,
                    Description = x.Description,
                    Date = x.Date,
                    IsEncrypted = x.IsEncrypted,
                    EncryptedDescription = x.EncryptedDescription,
                    Salt = x.Salt,
                    Nonce = x.Nonce,
                    Generated = x.Generated,
                    CreatedDate = x.CreatedDate,
                    ModifiedDate = x.ModifiedDate
                ))

        let upcomingExpenses =
            model.UpcomingExpenses
            |> Seq.map (fun x ->
                UpcomingExpense(
                    Id = x.Id,
                    UserId = userId,
                    CategoryId = x.CategoryId,
                    Amount = x.Amount,
                    Currency = x.Currency,
                    Description = x.Description,
                    Date = x.Date,
                    Generated = x.Generated,
                    CreatedDate = x.CreatedDate,
                    ModifiedDate = x.ModifiedDate
                ))

        let debts =
            model.Debts
            |> Seq.map (fun x ->
                Debt(
                    Id = x.Id,
                    UserId = userId,
                    Person = x.Person.Trim(),
                    Amount = x.Amount,
                    Currency = x.Currency,
                    UserIsDebtor = x.UserIsDebtor,
                    Description = x.Description,
                    CreatedDate = x.CreatedDate,
                    ModifiedDate = x.ModifiedDate
                ))

        let automaticTransactions =
            model.AutomaticTransactions
            |> Seq.map (fun x ->
                AutomaticTransaction(
                    Id = x.Id,
                    UserId = userId,
                    IsDeposit = x.IsDeposit,
                    CategoryId = x.CategoryId,
                    Amount = x.Amount,
                    Currency = x.Currency,
                    Description = x.Description,
                    DayInMonth = x.DayInMonth,
                    CreatedDate = x.CreatedDate,
                    ModifiedDate = x.ModifiedDate
                ))

        (accounts, categories, transactions, upcomingExpenses, debts, automaticTransactions)
