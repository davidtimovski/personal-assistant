namespace Accountant.Persistence.Fs

open System
open System.Threading.Tasks
open Npgsql.FSharp
open Accountant.Domain.Models

module AutomaticTransactionsRepository =

    let getAll (userId: int, fromModifiedDate: DateTime, connectionString: string) : Task<AutomaticTransaction list> =
        connectionString
        |> Sql.connect
        |> Sql.query "SELECT * FROM accountant.automatic_transactions WHERE user_id = @userId AND modified_date > @fromModifiedDate"
        |> Sql.parameters [
            "userId", Sql.int userId
            "fromModifiedDate", Sql.date fromModifiedDate ]
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
