namespace Accountant.Persistence.Fs

open System
open Npgsql.FSharp
open Accountant.Domain.Models

module DebtsRepository =

    let getAll (userId: int) (fromModifiedDate: DateTime) connectionString =
        connectionString
        |> Sql.connect
        |> Sql.query "SELECT * FROM accountant.debts WHERE user_id = @userId AND modified_date > @fromModifiedDate"
        |> Sql.parameters [
            "userId", Sql.int userId
            "fromModifiedDate", Sql.date fromModifiedDate ]
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
