namespace Accountant.Persistence.Fs

open System
open System.Threading.Tasks
open Npgsql.FSharp
open Accountant.Domain.Models

module UpcomingExpensesRepository =

    let getAll (userId: int, fromModifiedDate: DateTime, connectionString: string) : Task<UpcomingExpense list> =
        connectionString
        |> Sql.connect
        |> Sql.query "SELECT * FROM accountant.upcoming_expenses WHERE user_id = @userId AND modified_date > @fromModifiedDate"
        |> Sql.parameters [
            "userId", Sql.int userId
            "fromModifiedDate", Sql.date fromModifiedDate ]
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
