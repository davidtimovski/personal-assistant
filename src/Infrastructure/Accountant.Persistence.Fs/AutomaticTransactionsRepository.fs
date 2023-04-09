namespace Accountant.Persistence.Fs

open System
open System.Linq
open Microsoft.EntityFrameworkCore
open Npgsql.FSharp
open Accountant.Domain.Models
open CommonRepository

module AutomaticTransactionsRepository =

    let getAll (userId: int) (fromModifiedDate: DateTime) connectionString =
        connectionString
        |> Sql.connect
        |> Sql.query "SELECT * FROM accountant.automatic_transactions WHERE user_id = @userId AND modified_date > @fromModifiedDate"
        |> Sql.parameters [
            "userId", Sql.int userId
            "fromModifiedDate", Sql.timestamptz fromModifiedDate ]
        |> Sql.executeAsync (fun read ->
            {
                Id = read.int "id"
                UserId = read.int "user_id"
                IsDeposit = read.bool "is_deposit"
                CategoryId = read.intOrNone "category_id"
                Amount = read.decimal "amount"
                Currency = read.string "currency"
                Description = read.stringOrNone "description"
                DayInMonth = read.int16 "day_in_month"
                CreatedDate = read.dateTime "created_date"
                ModifiedDate = read.dateTime "modified_date"
            })

    let create (automaticTransaction: AutomaticTransaction) (ctx: AccountantContext) =
        task {
            let entity: Entities.AutomaticTransaction = 
                { Id = 0
                  UserId = automaticTransaction.UserId
                  IsDeposit = automaticTransaction.IsDeposit
                  CategoryId = 
                      match automaticTransaction.CategoryId with
                      | None -> Nullable<_>()
                      | Some x -> Nullable<_>(x)
                  Amount = automaticTransaction.Amount
                  Currency = automaticTransaction.Currency
                  Description =
                      match automaticTransaction.Description with
                      | None -> null
                      | Some x -> x
                  DayInMonth = automaticTransaction.DayInMonth
                  CreatedDate = automaticTransaction.CreatedDate
                  ModifiedDate = automaticTransaction.ModifiedDate }

            ctx.AutomaticTransactions.Add(entity) |> ignore
            let! _ = ctx.SaveChangesAsync true |> Async.AwaitTask

            return entity.Id
        }

    let update (automaticTransaction: AutomaticTransaction) (ctx: AccountantContext) =
        task {
            let dbAutomaticTransaction = ctx.AutomaticTransactions.AsNoTracking().First(fun x -> x.Id = automaticTransaction.Id && x.UserId = automaticTransaction.UserId)

            let entity: Entities.AutomaticTransaction = 
                { Id = dbAutomaticTransaction.Id
                  UserId = dbAutomaticTransaction.UserId
                  IsDeposit = automaticTransaction.IsDeposit
                  CategoryId = 
                      match automaticTransaction.CategoryId with
                      | None -> Nullable<_>()
                      | Some x -> Nullable<_>(x)
                  Amount = automaticTransaction.Amount
                  Currency = automaticTransaction.Currency
                  Description =
                      match automaticTransaction.Description with
                      | None -> null
                      | Some x -> x
                  DayInMonth = automaticTransaction.DayInMonth
                  CreatedDate = dbAutomaticTransaction.CreatedDate
                  ModifiedDate = automaticTransaction.ModifiedDate }

            ctx.Attach(entity) |> ignore
            ctx.Entry(entity).State <- EntityState.Modified
 
            ctx.SaveChangesAsync true
                |> Async.AwaitTask
                |> ignore
        }

    let delete (id: int) (userId: int) (ctx: AccountantContext) =
        task {
            CommonRepository.addDeletedEntity userId id EntityType.AutomaticTransaction ctx

            let entity = ctx.AutomaticTransactions.First(fun x -> x.Id = id && x.UserId = userId)

            ctx.Remove(entity) |> ignore
 
            ctx.SaveChangesAsync true
                |> Async.AwaitTask
                |> ignore
        }
