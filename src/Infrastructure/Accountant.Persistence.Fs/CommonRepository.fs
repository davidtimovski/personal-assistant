namespace Accountant.Persistence.Fs

open System
open System.Threading.Tasks
open EntityFrameworkCore.FSharp.Extensions
open Microsoft.EntityFrameworkCore
open Npgsql.FSharp
open Accountant.Domain.Models

module CommonRepository =

    type AccountantContext (options: DbContextOptions<AccountantContext>) =
        inherit DbContext(options)

        [<DefaultValue>]
        val mutable private _categories: DbSet<Category>
        member this.Categories with get() = this._categories and set v = this._categories <- v

        [<DefaultValue>]
        val mutable private _accounts : DbSet<Account>
        member this.Accounts with get() = this._accounts and set v = this._accounts <- v

        [<DefaultValue>]
        val mutable private _transactions : DbSet<Transaction>
        member this.Transactions with get() = this._transactions and set v = this._transactions <- v

        [<DefaultValue>]
        val mutable private _upcomingExpenses : DbSet<UpcomingExpense>
        member this.UpcomingExpenses with get() = this._upcomingExpenses and set v = this._upcomingExpenses <- v

        [<DefaultValue>]
        val mutable private _debts : DbSet<Debt>
        member this.Debts with get() = this._debts and set v = this._debts <- v

        [<DefaultValue>]
        val mutable private _automaticTransactions : DbSet<AutomaticTransaction>
        member this.AutomaticTransactions with get() = this._automaticTransactions and set v = this._automaticTransactions <- v

        [<DefaultValue>]
        val mutable private _deletedEntities: DbSet<DeletedEntity>
        member this.DeletedEntities with get() = this._deletedEntities and set v = this._deletedEntities <- v


        override _.OnModelCreating builder =
            builder.RegisterOptionTypes() // Enables option values for all entities

            builder.Entity<Category>(fun x -> 
                x.ToTable("categories", "accountant") |> ignore
            ) |> ignore

            builder.Entity<Account>(fun x -> 
                x.ToTable("accounts", "accountant") |> ignore
            ) |> ignore

            builder.Entity<Transaction>(fun x -> 
                x.ToTable("transactions", "accountant") |> ignore
            ) |> ignore

            builder.Entity<UpcomingExpense>(fun x -> 
                x.ToTable("upcoming_expenses", "accountant") |> ignore
            ) |> ignore

            builder.Entity<Debt>(fun x -> 
                x.ToTable("debts", "accountant") |> ignore
            ) |> ignore

            builder.Entity<AutomaticTransaction>(fun x -> 
                x.ToTable("automatic_transactions", "accountant") |> ignore
            ) |> ignore

            builder.Entity<DeletedEntity>(fun x -> 
                x.ToTable("deleted_entities", "accountant") |> ignore
                x.HasKey(fun e -> (e.UserId, e.EntityType, e.EntityId) :> obj) |> ignore
            ) |> ignore


    let getDeletedIds (userId: int) (fromDate: DateTime) (entityType: EntityType) (connectionString: string) : Task<int list> =
        connectionString
        |> Sql.connect
        |> Sql.query "SELECT entity_id FROM accountant.deleted_entities WHERE user_id = @userId AND entity_type = @entityType AND deleted_date > @fromDate"
        |> Sql.parameters [
            "userId", Sql.int userId
            "entityType", Sql.int16 (int16 entityType)
            "fromDate", Sql.date fromDate ]
        |> Sql.executeAsync (fun read -> read.int "entity_id")

    let addDeletedEntity (userId: int) (entityId: int) (entityType: EntityType) (ctx: AccountantContext) =
        ctx.Add({ 
            UserId = userId
            EntityType = entityType
            EntityId = entityId
            DeletedDate = DateTime.UtcNow }) |> ignore
