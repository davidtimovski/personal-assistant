namespace Accountant.Persistence.Fs

open System
open System.Linq
open Microsoft.EntityFrameworkCore
open Npgsql.FSharp
open Accountant.Domain.Models
open CommonRepository

module DebtsRepository =

    let getAll (userId: int) (fromModifiedDate: DateTime) connectionString =
        connectionString
        |> Sql.connect
        |> Sql.query "SELECT * FROM accountant.debts WHERE user_id = @userId AND modified_date > @fromModifiedDate"
        |> Sql.parameters [
            "userId", Sql.int userId
            "fromModifiedDate", Sql.timestamptz fromModifiedDate ]
        |> Sql.executeAsync (fun read ->
            {
                Id = read.int "id"
                UserId = read.int "user_id"
                Person = read.string "person"
                Amount = read.decimal "amount"
                Currency = read.string "currency"
                UserIsDebtor = read.bool "user_is_debtor"
                Description = read.stringOrNone "description"
                CreatedDate = read.dateTime "created_date"
                ModifiedDate = read.dateTime "modified_date"
            })

    let create (debt: Debt) (ctx: AccountantContext) =
        task {
            let entity: Entities.Debt = 
                { Id = 0
                  UserId = debt.UserId
                  Person = debt.Person
                  Amount = debt.Amount
                  Currency = debt.Currency
                  UserIsDebtor = debt.UserIsDebtor
                  Description =
                      match debt.Description with
                      | None -> null
                      | Some x -> x
                  CreatedDate = debt.CreatedDate
                  ModifiedDate = debt.ModifiedDate }

            ctx.Debts.Add(entity) |> ignore
            let! _ = ctx.SaveChangesAsync true |> Async.AwaitTask

            return entity.Id
        }

    let createMerged (debt: Debt) (ctx: AccountantContext) =
        task {
            let otherDebtWithPerson = ctx.Debts.Where(fun x -> x.UserId = debt.UserId && x.Person.ToLower() = debt.Person.ToLower()).ToList()

            for otherDebt in otherDebtWithPerson do
                ctx.Debts.Remove(otherDebt) |> ignore
                CommonRepository.addDeletedEntity debt.UserId otherDebt.Id EntityType.Debt ctx

            let entity: Entities.Debt = 
                { Id = 0
                  UserId = debt.UserId
                  Person = debt.Person
                  Amount = debt.Amount
                  Currency = debt.Currency
                  UserIsDebtor = debt.UserIsDebtor
                  Description =
                      match debt.Description with
                      | None -> null
                      | Some x -> x
                  CreatedDate = debt.CreatedDate
                  ModifiedDate = debt.ModifiedDate }

            ctx.Debts.Add(entity) |> ignore
            let! _ = ctx.SaveChangesAsync true |> Async.AwaitTask

            return entity.Id
        }

    let update (debt: Debt) (ctx: AccountantContext) =
        task {
            let dbDebt = ctx.Debts.AsNoTracking().First(fun x -> x.Id = debt.Id && x.UserId = debt.UserId)

            let entity: Entities.Debt = 
                { Id = dbDebt.Id
                  UserId = dbDebt.UserId
                  Person = debt.Person
                  Amount = debt.Amount
                  Currency = debt.Currency
                  UserIsDebtor = debt.UserIsDebtor
                  Description =
                      match debt.Description with
                      | None -> null
                      | Some x -> x
                  CreatedDate = dbDebt.CreatedDate
                  ModifiedDate = debt.ModifiedDate }

            ctx.Attach(entity) |> ignore
            ctx.Entry(entity).State <- EntityState.Modified
 
            ctx.SaveChangesAsync true
                |> Async.AwaitTask
                |> ignore
        }

    let delete (id: int) (userId: int) (ctx: AccountantContext) =
        task {
            CommonRepository.addDeletedEntity userId id EntityType.Debt ctx

            let entity = ctx.Debts.First(fun x -> x.Id = id && x.UserId = userId)

            ctx.Remove(entity) |> ignore
 
            ctx.SaveChangesAsync true
                |> Async.AwaitTask
                |> ignore
        }
