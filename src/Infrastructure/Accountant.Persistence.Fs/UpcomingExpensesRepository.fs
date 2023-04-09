namespace Accountant.Persistence.Fs

open System
open System.Linq
open Microsoft.EntityFrameworkCore
open Npgsql.FSharp
open Accountant.Domain.Models
open CommonRepository

module UpcomingExpensesRepository =

    let getAll (userId: int) (fromModifiedDate: DateTime) connectionString =
        connectionString
        |> Sql.connect
        |> Sql.query "SELECT * FROM accountant.upcoming_expenses WHERE user_id = @userId AND modified_date > @fromModifiedDate"
        |> Sql.parameters [
            "userId", Sql.int userId
            "fromModifiedDate", Sql.timestamptz fromModifiedDate ]
        |> Sql.executeAsync (fun read ->
            {
                Id = read.int "id"
                UserId = read.int "user_id"
                CategoryId = read.intOrNone "category_id"
                Amount = read.decimal "amount"
                Currency = read.string "currency"
                Description = read.stringOrNone "description"
                Date = read.dateTime "date"
                Generated = read.bool "generated"
                CreatedDate = read.dateTime "created_date"
                ModifiedDate = read.dateTime "modified_date"
            })

    let create (upcomingExpense: UpcomingExpense) (ctx: AccountantContext) =
        task {
            let entity: Entities.UpcomingExpense = 
                { Id = 0
                  UserId = upcomingExpense.UserId
                  CategoryId = 
                      match upcomingExpense.CategoryId with
                      | None -> Nullable<_>()
                      | Some x -> Nullable<_>(x)
                  Amount = upcomingExpense.Amount
                  Currency = upcomingExpense.Currency
                  Description =
                      match upcomingExpense.Description with
                      | None -> null
                      | Some x -> x
                  Date = upcomingExpense.Date.ToUniversalTime()
                  Generated = upcomingExpense.Generated
                  CreatedDate = upcomingExpense.CreatedDate
                  ModifiedDate = upcomingExpense.ModifiedDate }

            ctx.UpcomingExpenses.Add(entity) |> ignore
            let! _ = ctx.SaveChangesAsync true |> Async.AwaitTask

            return entity.Id
        }

    let update (upcomingExpense: UpcomingExpense) (ctx: AccountantContext) =
        task {
            let dbUpcomingExpense = ctx.UpcomingExpenses.AsNoTracking().First(fun x -> x.Id = upcomingExpense.Id && x.UserId = upcomingExpense.UserId)

            let entity: Entities.UpcomingExpense = 
                { Id = dbUpcomingExpense.Id
                  UserId = dbUpcomingExpense.UserId
                  CategoryId = 
                      match upcomingExpense.CategoryId with
                      | None -> Nullable<_>()
                      | Some x -> Nullable<_>(x)
                  Amount = upcomingExpense.Amount
                  Currency = upcomingExpense.Currency
                  Description =
                      match upcomingExpense.Description with
                      | None -> null
                      | Some x -> x
                  Date = upcomingExpense.Date.ToUniversalTime()
                  Generated = upcomingExpense.Generated
                  CreatedDate = dbUpcomingExpense.CreatedDate
                  ModifiedDate = upcomingExpense.ModifiedDate }

            ctx.Attach(entity) |> ignore
            ctx.Entry(entity).State <- EntityState.Modified
 
            ctx.SaveChangesAsync true
                |> Async.AwaitTask
                |> ignore
        }

    let delete (id: int) (userId: int) (ctx: AccountantContext) =
        task {
            CommonRepository.addDeletedEntity userId id EntityType.UpcomingExpense ctx

            let entity = ctx.UpcomingExpenses.First(fun x -> x.Id = id && x.UserId = userId)

            ctx.Remove(entity) |> ignore
 
            ctx.SaveChangesAsync true
                |> Async.AwaitTask
                |> ignore
        }

    let deleteOld (userId: int) (before: DateTime) (ctx: AccountantContext) =
        task {
            let beforeUtc = before.ToUniversalTime()
            let toDelete = ctx.UpcomingExpenses.Where(fun x -> x.UserId = userId && x.Date < beforeUtc).ToList()

            for upcomingExpense in toDelete do
                CommonRepository.addDeletedEntity userId upcomingExpense.Id EntityType.UpcomingExpense ctx
                ctx.Remove(upcomingExpense) |> ignore
                
            ctx.SaveChangesAsync true
                |> Async.AwaitTask
                |> ignore
        }
