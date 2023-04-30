namespace Accountant.Persistence.Fs

open System
open Npgsql.FSharp
open ConnectionUtils
open Models
open Npgsql
open Sentry

module DebtsRepository =

    [<Literal>]
    let private table = "accountant.debts"

    let getAll (userId: int) (fromModifiedDate: DateTime) connectionString =
        connectionString
        |> Sql.connect
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

    let create (debt: Debt) (conn: RegularOrTransactionalConn) (metricsSpan: ISpan) =
        let metric = metricsSpan.StartChild("DebtsRepository.create")

        task {
            let! id =
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

            metric.Finish()

            return id
        }

    let createMerged (debt: Debt) connectionString (metricsSpan: ISpan) =
        let metric = metricsSpan.StartChild("DebtsRepository.createMerged")

        task {
            use conn = new NpgsqlConnection(connectionString)
            conn.Open()

            use tr = conn.BeginTransaction()

            let debtsIdsForPerson =
                connectionString
                |> Sql.connect
                |> Sql.query $"SELECT id FROM {table} WHERE user_id = @user_id AND LOWER(person) = @person"
                |> Sql.parameters
                    [ "user_id", Sql.int debt.UserId
                      "person", Sql.string (debt.Person.ToLowerInvariant()) ]
                |> Sql.execute (fun read -> read.int "id")

            let deleteQueries =
                debtsIdsForPerson
                |> List.map (fun id ->
                    $"INSERT INTO accountant.deleted_entities
                      (user_id, entity_type, entity_id, deleted_date) VALUES
                      (@user_id, @entity_type, {id}, @deleted_date);
                      DELETE FROM {table} WHERE id = {id} AND user_id = @user_id;")

            for query in deleteQueries do
                conn
                |> Sql.existingConnection
                |> Sql.query query
                |> Sql.parameters
                    [ "user_id", Sql.int debt.UserId
                      "entity_type", Sql.int (LanguagePrimitives.EnumToValue EntityType.Debt)
                      "deleted_date", Sql.timestamptz DateTime.UtcNow ]
                |> Sql.executeNonQuery
                |> ignore

            let! id =
                conn
                |> Sql.existingConnection
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

            tr.Commit()

            metric.Finish()

            return id
        }

    let update (debt: Debt) connectionString (metricsSpan: ISpan) =
        let metric = metricsSpan.StartChild("DebtsRepository.update")

        task {
            let! _ =
                connectionString
                |> Sql.connect
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

            metric.Finish()
        }

    let delete (id: int) (userId: int) connectionString (metricsSpan: ISpan) =
        let metric = metricsSpan.StartChild("DebtsRepository.delete")

        task {
            let! _ =
                connectionString
                |> Sql.connect
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

            metric.Finish()
        }
