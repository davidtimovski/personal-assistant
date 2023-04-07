namespace Accountant.Persistence.Fs

open System
open System.Linq
open Microsoft.EntityFrameworkCore
open Npgsql.FSharp
open Accountant.Domain.Models
open CommonRepository

module AccountsRepository =

    let getAll (userId: int) (fromModifiedDate: DateTime) connectionString =
        connectionString
        |> Sql.connect
        |> Sql.query "SELECT * FROM accountant.accounts WHERE user_id = @userId AND modified_date > @fromModifiedDate"
        |> Sql.parameters [
            "userId", Sql.int userId
            "fromModifiedDate", Sql.date fromModifiedDate ]
        |> Sql.executeAsync (fun read ->
            {
                Id = read.int "id"
                UserId = read.int "user_id"
                Name = read.string "name"
                IsMain = read.bool "is_main"
                Currency = read.string "currency"
                StockPrice = read.decimalOrNone "stock_price"
                CreatedDate = read.dateTime "created_date"
                ModifiedDate = read.dateTime "modified_date"
            })

    let exists (id: int) (userId: int) connectionString =
        connectionString
        |> Sql.connect
        |> Sql.query "SELECT COUNT(*) AS count FROM accountant.accounts WHERE id = @id AND user_id = @userId"
        |> Sql.parameters [
            "id", Sql.int id
            "userId", Sql.int userId ]
        |> Sql.executeRow (fun read -> (read.int "count") > 0)

    let hasMain (userId: int) connectionString =
        connectionString
        |> Sql.connect
        |> Sql.query "SELECT COUNT(*) AS count FROM accountant.accounts WHERE user_id = @userId AND is_main"
        |> Sql.parameters [
            "userId", Sql.int userId ]
        |> Sql.executeRow (fun read -> (read.int "count") > 0)

    let isMain (id: int) (userId: int) connectionString =
        connectionString
        |> Sql.connect
        |> Sql.query "SELECT COUNT(*) AS count FROM accountant.accounts WHERE id = @id AND user_id = @userId AND is_main"
        |> Sql.parameters [
            "id", Sql.int id
            "userId", Sql.int userId ]
        |> Sql.executeRow (fun read -> (read.int "count") > 0)

    let create (account: Account) (ctx: AccountantContext) =
        task {
            ctx.Accounts.Add(account) |> ignore
            let! _ = ctx.SaveChangesAsync true |> Async.AwaitTask
            return account.Id
        }

    let update (account: Account) (ctx: AccountantContext) =
        task {
            let dbAccount = ctx.Accounts.AsNoTracking().First(fun x -> x.Id = account.Id && x.UserId = account.UserId)

            let updatedAccount = 
                { Id = dbAccount.Id
                  UserId = dbAccount.UserId
                  Name = account.Name
                  IsMain = dbAccount.IsMain
                  Currency = account.Currency
                  StockPrice = account.StockPrice
                  CreatedDate = dbAccount.CreatedDate
                  ModifiedDate = account.ModifiedDate }

            ctx.Attach(updatedAccount) |> ignore
 
            ctx.SaveChangesAsync true
                |> Async.AwaitTask
                |> ignore
        }

    let delete (id: int) (userId: int) (ctx: AccountantContext) =
        task {
            CommonRepository.addDeletedEntity userId id EntityType.Account ctx

            let dbAccount = ctx.Accounts.First(fun x -> x.Id = id && x.UserId = userId)

            ctx.Remove(dbAccount) |> ignore
 
            ctx.SaveChangesAsync true
                |> Async.AwaitTask
                |> ignore
        }
