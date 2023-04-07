namespace Accountant.Persistence.Fs

open System
open Npgsql.FSharp
open Accountant.Domain.Models

module CategoriesRepository =

    let getAll (userId: int) (fromModifiedDate: DateTime) connectionString =
        connectionString
        |> Sql.connect
        |> Sql.query "SELECT * FROM accountant.categories WHERE user_id = @userId AND modified_date > @fromModifiedDate"
        |> Sql.parameters [
            "userId", Sql.int userId
            "fromModifiedDate", Sql.date fromModifiedDate ]
        |> Sql.executeAsync (fun read ->
            {
                Id = read.int "id"
                UserId = read.int "user_id"
                ParentId = read.intOrNone "parent_id"
                Name = read.string "name"
                Type = enum<CategoryType>(read.int "type")
                GenerateUpcomingExpense = read.bool "generate_upcoming_expense"
                IsTax = read.bool "is_tax"
                CreatedDate = read.dateTime "created_date"
                ModifiedDate = read.dateTime "modified_date"
            })
