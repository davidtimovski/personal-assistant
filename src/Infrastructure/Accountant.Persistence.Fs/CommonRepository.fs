namespace Accountant.Persistence.Fs

open System
open System.Threading.Tasks
open EntityFrameworkCore.FSharp.Extensions
open Microsoft.EntityFrameworkCore
open Npgsql.FSharp
open Accountant.Domain.Models

module CommonRepository =

    type AccountantContext(options: DbContextOptions<AccountantContext>) =
        inherit DbContext(options)

        [<DefaultValue>]
        val mutable private _categories: DbSet<Entities.Category>

        member this.Categories
            with internal get () = this._categories
            and internal set v = this._categories <- v

        [<DefaultValue>]
        val mutable private _accounts: DbSet<Entities.Account>

        member this.Accounts
            with internal get () = this._accounts
            and internal set v = this._accounts <- v

        [<DefaultValue>]
        val mutable private _transactions: DbSet<Entities.Transaction>

        member this.Transactions
            with internal get () = this._transactions
            and internal set v = this._transactions <- v

        [<DefaultValue>]
        val mutable private _upcomingExpenses: DbSet<Entities.UpcomingExpense>

        member this.UpcomingExpenses
            with internal get () = this._upcomingExpenses
            and internal set v = this._upcomingExpenses <- v

        [<DefaultValue>]
        val mutable private _debts: DbSet<Entities.Debt>

        member this.Debts
            with internal get () = this._debts
            and internal set v = this._debts <- v

        [<DefaultValue>]
        val mutable private _automaticTransactions: DbSet<Entities.AutomaticTransaction>

        member this.AutomaticTransactions
            with internal get () = this._automaticTransactions
            and internal set v = this._automaticTransactions <- v

        [<DefaultValue>]
        val mutable private _deletedEntities: DbSet<Entities.DeletedEntity>

        member this.DeletedEntities
            with internal get () = this._deletedEntities
            and internal set v = this._deletedEntities <- v


        override _.OnModelCreating builder =
            builder.RegisterOptionTypes() // Enables option values for all entities

            builder.Entity<Entities.Category>(fun x -> x.ToTable("categories", "accountant") |> ignore)
            |> ignore

            builder.Entity<Entities.Account>(fun x -> x.ToTable("accounts", "accountant") |> ignore)
            |> ignore

            builder.Entity<Entities.Transaction>(fun x -> x.ToTable("transactions", "accountant") |> ignore)
            |> ignore

            builder.Entity<Entities.UpcomingExpense>(fun x -> x.ToTable("upcoming_expenses", "accountant") |> ignore)
            |> ignore

            builder.Entity<Entities.Debt>(fun x -> x.ToTable("debts", "accountant") |> ignore)
            |> ignore

            builder.Entity<Entities.AutomaticTransaction>(fun x ->
                x.ToTable("automatic_transactions", "accountant") |> ignore)
            |> ignore

            builder.Entity<Entities.DeletedEntity>(fun x ->
                x.ToTable("deleted_entities", "accountant") |> ignore
                x.HasKey(fun e -> (e.UserId, e.EntityType, e.EntityId) :> obj) |> ignore)
            |> ignore


    let getDeletedIds
        (userId: int)
        (fromDate: DateTime)
        (entityType: EntityType)
        (connectionString: string)
        : Task<int list> =
        connectionString
        |> Sql.connect
        |> Sql.query
            "SELECT entity_id FROM accountant.deleted_entities WHERE user_id = @userId AND entity_type = @entityType AND deleted_date > @fromDate"
        |> Sql.parameters
            [ "userId", Sql.int userId
              "entityType", Sql.int16 (int16 entityType)
              "fromDate", Sql.date fromDate ]
        |> Sql.executeAsync (fun read -> read.int "entity_id")

    let addDeletedEntity (userId: int) (entityId: int) (entityType: EntityType) (ctx: AccountantContext) =
        ctx.Add(
            { UserId = userId
              EntityType = entityType
              EntityId = entityId
              DeletedDate = DateTime.UtcNow }
        )
        |> ignore

    let sync
        (accounts: seq<Account>)
        (categories: seq<Category>)
        (transactions: seq<Transaction>)
        (upcomingExpenses: seq<UpcomingExpense>)
        (debts: seq<Debt>)
        (automaticTransactions: seq<AutomaticTransaction>)
        (userId: int)
        (ctx: AccountantContext)
        =
        task {
            let accountEntities =
                accounts
                |> Seq.map (fun x ->
                    { Id = 0
                      UserId = userId
                      Name = x.Name
                      IsMain = x.IsMain
                      Currency = x.Currency
                      StockPrice =
                        match x.StockPrice with
                        | None -> Nullable<_>()
                        | Some x -> Nullable<_>(x)
                      CreatedDate = x.CreatedDate
                      ModifiedDate = x.ModifiedDate }: Entities.Account)
                |> Seq.toList

            ctx.Accounts.AddRange(accountEntities) |> ignore

            let categoryEntities =
                categories
                |> Seq.map (fun x ->
                    { Id = 0
                      UserId = userId
                      ParentId =
                        match x.ParentId with
                        | None -> Nullable<_>()
                        | Some x -> Nullable<_>(x)
                      Name = x.Name
                      Type = x.Type
                      GenerateUpcomingExpense = x.GenerateUpcomingExpense
                      IsTax = x.IsTax
                      CreatedDate = x.CreatedDate
                      ModifiedDate = x.ModifiedDate }: Entities.Category)
                |> Seq.toList

            ctx.Categories.AddRange(categoryEntities) |> ignore

            let transactionEntities =
                transactions
                |> Seq.map (fun x ->
                    { Id = 0
                      FromAccountId =
                        match x.FromAccountId with
                        | None -> Nullable<_>()
                        | Some x -> Nullable<_>(x)
                      ToAccountId =
                        match x.ToAccountId with
                        | None -> Nullable<_>()
                        | Some x -> Nullable<_>(x)
                      CategoryId =
                        match x.CategoryId with
                        | None -> Nullable<_>()
                        | Some x -> Nullable<_>(x)
                      Amount = x.Amount
                      FromStocks =
                        match x.FromStocks with
                        | None -> Nullable<_>()
                        | Some x -> Nullable<_>(x)
                      ToStocks =
                        match x.ToStocks with
                        | None -> Nullable<_>()
                        | Some x -> Nullable<_>(x)
                      Currency = x.Currency
                      Description =
                        match x.Description with
                        | None -> null
                        | Some x -> x
                      Date = x.Date.ToUniversalTime()
                      IsEncrypted = x.IsEncrypted
                      EncryptedDescription =
                        match x.EncryptedDescription with
                        | None -> Array.zeroCreate 0
                        | Some x -> x
                      Salt =
                        match x.Salt with
                        | None -> Array.zeroCreate 0
                        | Some x -> x
                      Nonce =
                        match x.Nonce with
                        | None -> Array.zeroCreate 0
                        | Some x -> x
                      Generated = x.Generated
                      CreatedDate = x.CreatedDate
                      ModifiedDate = x.ModifiedDate }: Entities.Transaction)
                |> Seq.toList

            ctx.Transactions.AddRange(transactionEntities) |> ignore

            let upcomingExpenseEntities =
                upcomingExpenses
                |> Seq.map (fun x ->
                    { Id = 0
                      UserId = userId
                      CategoryId =
                        match x.CategoryId with
                        | None -> Nullable<_>()
                        | Some x -> Nullable<_>(x)
                      Amount = x.Amount
                      Currency = x.Currency
                      Description =
                        match x.Description with
                        | None -> null
                        | Some x -> x
                      Date = x.Date.ToUniversalTime()
                      Generated = x.Generated
                      CreatedDate = x.CreatedDate
                      ModifiedDate = x.ModifiedDate }: Entities.UpcomingExpense)
                |> Seq.toList

            ctx.UpcomingExpenses.AddRange(upcomingExpenseEntities) |> ignore

            let debtEntities =
                debts
                |> Seq.map (fun x ->
                    { Id = 0
                      UserId = userId
                      Person = x.Person
                      Amount = x.Amount
                      Currency = x.Currency
                      UserIsDebtor = x.UserIsDebtor
                      Description =
                        match x.Description with
                        | None -> null
                        | Some x -> x
                      CreatedDate = x.CreatedDate
                      ModifiedDate = x.ModifiedDate }: Entities.Debt)
                |> Seq.toList

            ctx.Debts.AddRange(debtEntities) |> ignore

            let automaticTransactionEntities =
                automaticTransactions
                |> Seq.map (fun x ->
                    { Id = 0
                      UserId = userId
                      IsDeposit = x.IsDeposit
                      CategoryId =
                        match x.CategoryId with
                        | None -> Nullable<_>()
                        | Some x -> Nullable<_>(x)
                      Amount = x.Amount
                      Currency = x.Currency
                      Description =
                        match x.Description with
                        | None -> null
                        | Some x -> x
                      DayInMonth = x.DayInMonth
                      CreatedDate = x.CreatedDate
                      ModifiedDate = x.ModifiedDate }: Entities.AutomaticTransaction)
                |> Seq.toList

            ctx.AutomaticTransactions.AddRange(automaticTransactionEntities) |> ignore

            let! _ = ctx.SaveChangesAsync true |> Async.AwaitTask

            let accountIds = accountEntities |> Seq.map (fun x -> x.Id) |> Seq.toArray
            let categoryIds = categoryEntities |> Seq.map (fun x -> x.Id) |> Seq.toArray
            let transactionIds = transactionEntities |> Seq.map (fun x -> x.Id) |> Seq.toArray

            let upcomingExpenseIds =
                upcomingExpenseEntities |> Seq.map (fun x -> x.Id) |> Seq.toArray

            let debtIds = debtEntities |> Seq.map (fun x -> x.Id) |> Seq.toArray

            let automaticTransactionIds =
                automaticTransactionEntities |> Seq.map (fun x -> x.Id) |> Seq.toArray

            return (accountIds, categoryIds, transactionIds, upcomingExpenseIds, debtIds, automaticTransactionIds)
        }
