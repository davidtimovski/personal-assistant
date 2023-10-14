namespace Accountant.Persistence

open System
open Npgsql.FSharp
open ConnectionUtils
open Models
open Sentry

module UpcomingExpensesRepository =

    [<Literal>]
    let private table = "accountant.upcoming_expenses"

    let getAll (userId: int) (fromModifiedDate: DateTime) connectionString =
        connectionString
        |> Sql.connect
        |> Sql.query $"SELECT * FROM {table} WHERE user_id = @user_id AND modified_date > @modified_date"
        |> Sql.parameters [ "user_id", Sql.int userId; "modified_date", Sql.timestamptz fromModifiedDate ]
        |> Sql.executeAsync (fun read ->
            { Id = read.int "id"
              UserId = read.int "user_id"
              CategoryId = read.intOrNone "category_id"
              Amount = read.decimal "amount"
              Currency = read.string "currency"
              Description = read.stringOrNone "description"
              Date = read.dateTime "date"
              Generated = read.bool "generated"
              CreatedDate = read.dateTime "created_date"
              ModifiedDate = read.dateTime "modified_date" })

    let create (upcomingExpense: UpcomingExpense) (conn: RegularOrTransactionalConn) (metricsSpan: ISpan) =
        let metric = metricsSpan.StartChild("UpcomingExpensesRepository.create")

        task {
            let! id =
                ConnectionUtils.connect conn
                |> Sql.query
                    $"INSERT INTO {table}
                          (user_id, category_id, amount, currency, description, date, generated, created_date, modified_date) VALUES 
                          (@user_id, @category_id, @amount, @currency, @description, @date, @generated, @created_date, @modified_date) RETURNING id"
                |> Sql.parameters
                    [ "user_id", Sql.int upcomingExpense.UserId
                      "category_id", Sql.intOrNone upcomingExpense.CategoryId
                      "amount", Sql.decimal upcomingExpense.Amount
                      "currency", Sql.string upcomingExpense.Currency
                      "description", Sql.stringOrNone upcomingExpense.Description
                      "date", Sql.date (upcomingExpense.Date.ToUniversalTime())
                      "generated", Sql.bool upcomingExpense.Generated
                      "created_date", Sql.timestamptz upcomingExpense.CreatedDate
                      "modified_date", Sql.timestamptz upcomingExpense.ModifiedDate ]
                |> Sql.executeRowAsync (fun read -> read.int "id")

            metric.Finish()

            return id
        }

    let update (upcomingExpense: UpcomingExpense) connectionString (metricsSpan: ISpan) =
        let metric = metricsSpan.StartChild("UpcomingExpensesRepository.update")

        task {
            let! _ =
                connectionString
                |> Sql.connect
                |> Sql.query
                    $"UPDATE {table}
                      SET category_id = @category_id, amount = @amount, currency = @currency, description = @description, date = @date, modified_date = @modified_date
                      WHERE id = @id AND user_id = @user_id"
                |> Sql.parameters
                    [ "id", Sql.int upcomingExpense.Id
                      "category_id", Sql.intOrNone upcomingExpense.CategoryId
                      "amount", Sql.decimal upcomingExpense.Amount
                      "currency", Sql.string upcomingExpense.Currency
                      "description", Sql.stringOrNone upcomingExpense.Description
                      "date", Sql.date (upcomingExpense.Date.ToUniversalTime())
                      "modified_date", Sql.timestamptz upcomingExpense.ModifiedDate ]
                |> Sql.executeNonQueryAsync

            metric.Finish()
        }

    let delete (id: int) (userId: int) connectionString (metricsSpan: ISpan) =
        let metric = metricsSpan.StartChild("UpcomingExpensesRepository.delete")

        task {
            let! _ =
                connectionString
                |> Sql.connect
                |> Sql.executeTransactionAsync
                    [ "INSERT INTO accountant.deleted_entities
                            (user_id, entity_type, entity_id, deleted_date) VALUES
                            (@user_id, @entity_type, @entity_id, @deleted_date)",
                      [ [ "user_id", Sql.int userId
                          "entity_type", Sql.int (LanguagePrimitives.EnumToValue EntityType.UpcomingExpense)
                          "entity_id", Sql.int id
                          "deleted_date", Sql.timestamptz DateTime.UtcNow ] ]

                      $"DELETE FROM {table} WHERE id = @id AND user_id = @user_id",
                      [ [ "id", Sql.int id; "user_id", Sql.int userId ] ] ]

            metric.Finish()
        }

    let deleteOld (userId: int) (before: DateTime) connectionString (metricsSpan: ISpan) =
        let metric = metricsSpan.StartChild("UpcomingExpensesRepository.deleteOld")

        task {
            let beforeUtc = before.ToUniversalTime()

            let oldUpcomingExpenseIds =
                connectionString
                |> Sql.connect
                |> Sql.query $"SELECT id FROM {table} WHERE user_id = @user_id AND date < @date"
                |> Sql.parameters [ "user_id", Sql.int userId; "date", Sql.timestamptz beforeUtc ]
                |> Sql.execute (fun read -> read.int "id")

            let deletedEntitiesInsertParams =
                oldUpcomingExpenseIds
                |> List.map (fun x ->
                    [ "user_id", Sql.int userId
                      "entity_type", Sql.int (LanguagePrimitives.EnumToValue EntityType.UpcomingExpense)
                      "entity_id", Sql.int x
                      "deleted_date", Sql.timestamptz DateTime.UtcNow ])

            let! _ =
                connectionString
                |> Sql.connect
                |> Sql.executeTransactionAsync
                    [ "INSERT INTO accountant.deleted_entities
                        (user_id, entity_type, entity_id, deleted_date) VALUES
                        (@user_id, @entity_type, @entity_id, @deleted_date)",
                      deletedEntitiesInsertParams

                      $"DELETE FROM {table} WHERE user_id = @user_id AND date < @date",
                      [ [ "user_id", Sql.int userId; "date", Sql.timestamptz beforeUtc ] ] ]

            metric.Finish()
        }
