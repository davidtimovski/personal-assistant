namespace Accountant.Persistence.Fs

open System
open Npgsql.FSharp
open Accountant.Domain.Models
open ConnectionUtils

module DebtsRepository =

    [<Literal>]
    let private table = "accountant.debts"

    let getAll (userId: int) (fromModifiedDate: DateTime) (conn: RegularOrTransactionalConn) =
        ConnectionUtils.connect conn
        |> Sql.query $"SELECT * FROM {table} WHERE user_id = @user_id AND modified_date > @modified_date"
        |> Sql.parameters [ "user_id", Sql.int userId; "modified_date", Sql.timestamptz fromModifiedDate ]
        |> Sql.executeAsync (fun read ->
            { Id = read.int "id"
              UserId = read.int "user_id"
              Person = read.string "person"
              Amount = read.decimal "amount"
              Currency = read.string "currency"
              UserIsDebtor = read.bool "user_is_debtor"
              Description = read.stringOrNone "description"
              CreatedDate = read.dateTime "created_date"
              ModifiedDate = read.dateTime "modified_date" })

    let create (debt: Debt) (conn: RegularOrTransactionalConn) =
        task {
            return!
                ConnectionUtils.connect conn
                |> Sql.query
                    $"INSERT INTO {table}
                           (user_id, person, amount, currency, description, user_is_debtor, created_date, modified_date) VALUES 
                           (@user_id, @person, @amount, @currency, @description, @user_is_debtor, @created_date, @modified_date) RETURNING id"
                |> Sql.parameters
                    [ "user_id", Sql.int debt.UserId
                      "person", Sql.string debt.Person
                      "amount", Sql.decimal debt.Amount
                      "currency", Sql.string debt.Currency
                      "description", Sql.stringOrNone debt.Description
                      "user_is_debtor", Sql.bool debt.UserIsDebtor
                      "created_date", Sql.timestamptz debt.CreatedDate
                      "modified_date", Sql.timestamptz debt.ModifiedDate ]
                |> Sql.executeRowAsync (fun read -> read.int "id")
                |> Async.AwaitTask
        }

    let createMerged (debt: Debt) (conn: RegularOrTransactionalConn) =
        task {
            let debtsIdsForPerson =
                ConnectionUtils.connect conn
                |> Sql.query $"SELECT id FROM {table} WHERE user_id = @user_id AND LOWER(person) = @person"
                |> Sql.parameters
                    [ "user_id", Sql.int debt.UserId
                      "person", Sql.string (debt.Person.ToLowerInvariant()) ]
                |> Sql.execute (fun read -> read.int "id")

            let now = DateTime.UtcNow

            let deletedEntitiesInsertParams =
                debtsIdsForPerson
                |> List.map (fun x ->
                    [ "user_id", Sql.int debt.UserId
                      "entity_type", Sql.int (LanguagePrimitives.EnumToValue EntityType.Debt)
                      "entity_id", Sql.int x
                      "deleted_date", Sql.timestamptz now ])

            let! ids =
                ConnectionUtils.connect conn
                |> Sql.executeTransactionAsync
                    [ $"INSERT INTO {table}
                           (user_id, person, amount, currency, description, user_is_debtor, created_date, modified_date) VALUES 
                           (@user_id, @person, @amount, @currency, @description, @user_is_debtor, @created_date, @modified_date) RETURNING id",
                      [ [ "user_id", Sql.int debt.UserId
                          "person", Sql.string debt.Person
                          "amount", Sql.decimal debt.Amount
                          "currency", Sql.string debt.Currency
                          "description", Sql.stringOrNone debt.Description
                          "user_is_debtor", Sql.bool debt.UserIsDebtor
                          "created_date", Sql.timestamptz debt.CreatedDate
                          "modified_date", Sql.timestamptz debt.ModifiedDate ] ]

                      "INSERT INTO accountant.deleted_entities
                      (user_id, entity_type, entity_id, deleted_date) VALUES
                      (@user_id, @entity_type, @entity_id, @deleted_date)",
                      deletedEntitiesInsertParams

                      $"DELETE FROM {table} WHERE user_id = @user_id AND LOWER(person) = @person",
                      [ [ "user_id", Sql.int debt.UserId; "person", Sql.string (debt.Person.ToLowerInvariant()) ] ] ]
                |> Async.AwaitTask

            return ids[0]
        }

    let update (debt: Debt) (conn: RegularOrTransactionalConn) =
        task {
            return!
                ConnectionUtils.connect conn
                |> Sql.query
                    $"UPDATE {table}
                           SET person = @person, amount = @amount, currency = @currency, description = @description, user_is_debtor = @user_is_debtor, modified_date = @modified_date
                           WHERE id = @id AND user_id = @user_id"
                |> Sql.parameters
                    [ "id", Sql.int debt.Id
                      "user_id", Sql.int debt.UserId
                      "person", Sql.string debt.Person
                      "amount", Sql.decimal debt.Amount
                      "currency", Sql.string debt.Currency
                      "description", Sql.stringOrNone debt.Description
                      "user_is_debtor", Sql.bool debt.UserIsDebtor
                      "modified_date", Sql.timestamptz debt.ModifiedDate ]
                |> Sql.executeNonQueryAsync
                |> Async.AwaitTask
        }

    let delete (id: int) (userId: int) (conn: RegularOrTransactionalConn) =
        task {
            ConnectionUtils.connect conn
            |> Sql.executeTransactionAsync
                [ "INSERT INTO accountant.deleted_entities
                      (user_id, entity_type, entity_id, deleted_date) VALUES
                      (@user_id, @entity_type, @entity_id, @deleted_date)",
                  [ [ "user_id", Sql.int userId
                      "entity_type", Sql.int (LanguagePrimitives.EnumToValue EntityType.Debt)
                      "entity_id", Sql.int id
                      "deleted_date", Sql.timestamptz DateTime.UtcNow ] ]

                  $"DELETE FROM {table} WHERE id = @id AND user_id = @user_id",
                  [ [ "id", Sql.int id; "user_id", Sql.int userId ] ] ]
            |> Async.AwaitTask
            |> ignore
        }
