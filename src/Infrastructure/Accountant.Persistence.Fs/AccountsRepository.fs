namespace Accountant.Persistence.Fs

open System
open System.Threading.Tasks
open Npgsql.FSharp
open Accountant.Domain.Models

module AccountsRepository =

    let getAll (userId: int, fromModifiedDate: DateTime, connectionString: string) : Task<Account list> =
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
