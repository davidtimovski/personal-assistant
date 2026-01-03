namespace Accountant.Persistence

open System
open Npgsql.FSharp
open ConnectionUtils
open Models
open Sentry

module AutomaticTransactionsRepository =

    [<Literal>]
    let private table = "accountant.automatic_transactions"

    let private rowToEntity (read: RowReader) =
        { Id = read.int "id"
          UserId = read.int "user_id"
          IsDeposit = read.bool "is_deposit"
          CategoryId = read.intOrNone "category_id"
          Amount = read.decimal "amount"
          Currency = read.string "currency"
          Description = read.stringOrNone "description"
          DayInMonth = read.int16 "day_in_month"
          CreatedDate = read.dateTime "created_date"
          ModifiedDate = read.dateTime "modified_date" }

    let get (id: int) connectionString =
        task {
            let! automaticTransactions =
                connectionString
                |> Sql.connect
                |> Sql.query $"SELECT * FROM {table} WHERE id = @id"
                |> Sql.parameters [ "id", Sql.int id ]
                |> Sql.executeAsync rowToEntity
            
            return if automaticTransactions.Length > 0 then Some automaticTransactions[0] else None
        }

    let getAll (userId: int) (fromModifiedDate: DateTime) connectionString =
        connectionString
        |> Sql.connect
        |> Sql.query $"SELECT * FROM {table} WHERE user_id = @user_id AND modified_date > @modified_date"
        |> Sql.parameters [ "user_id", Sql.int userId; "modified_date", Sql.timestamptz fromModifiedDate ]
        |> Sql.executeAsync rowToEntity

    let create (automaticTransaction: AutomaticTransaction) (conn: RegularOrTransactionalConn) (metricsSpan: ISpan) =
        let metric = metricsSpan.StartChild("AutomaticTransactionsRepository.create")

        task {
            let! id =
                ConnectionUtils.connect conn
                |> Sql.query
                    $"INSERT INTO {table}
                            (user_id, is_deposit, category_id, amount, currency, description, day_in_month, created_date, modified_date) VALUES 
                            (@user_id, @is_deposit, @category_id, @amount, @currency, @description, @day_in_month, @created_date, @modified_date) RETURNING id"
                |> Sql.parameters
                    [ "user_id", Sql.int automaticTransaction.UserId
                      "is_deposit", Sql.bool automaticTransaction.IsDeposit
                      "category_id", Sql.intOrNone automaticTransaction.CategoryId
                      "amount", Sql.decimal automaticTransaction.Amount
                      "currency", Sql.string automaticTransaction.Currency
                      "description", Sql.stringOrNone automaticTransaction.Description
                      "day_in_month", Sql.int16 automaticTransaction.DayInMonth
                      "created_date", Sql.timestamptz automaticTransaction.CreatedDate
                      "modified_date", Sql.timestamptz automaticTransaction.ModifiedDate ]
                |> Sql.executeRowAsync (fun read -> read.int "id")

            metric.Finish()

            return id
        }

    let update (automaticTransaction: AutomaticTransaction) connectionString (metricsSpan: ISpan) =
        let metric = metricsSpan.StartChild("AutomaticTransactionsRepository.update")

        task {
            let! _ =
                connectionString
                |> Sql.connect
                |> Sql.query
                    $"UPDATE {table}
                      SET is_deposit = @is_deposit, category_id = @category_id, amount = @amount, currency = @currency, description = @description, day_in_month = @day_in_month, modified_date = @modified_date
                      WHERE id = @id AND user_id = @user_id"
                |> Sql.parameters
                    [ "id", Sql.int automaticTransaction.Id
                      "user_id", Sql.int automaticTransaction.UserId
                      "is_deposit", Sql.bool automaticTransaction.IsDeposit
                      "category_id", Sql.intOrNone automaticTransaction.CategoryId
                      "amount", Sql.decimal automaticTransaction.Amount
                      "currency", Sql.string automaticTransaction.Currency
                      "description", Sql.stringOrNone automaticTransaction.Description
                      "day_in_month", Sql.int16 automaticTransaction.DayInMonth
                      "modified_date", Sql.timestamptz automaticTransaction.ModifiedDate ]
                |> Sql.executeNonQueryAsync

            metric.Finish()
        }

    let delete (id: int) (userId: int) connectionString (metricsSpan: ISpan) =
        let metric = metricsSpan.StartChild("AutomaticTransactionsRepository.delete")

        task {
            let! _ =
                connectionString
                |> Sql.connect
                |> Sql.executeTransactionAsync
                    [ "INSERT INTO accountant.deleted_entities
                            (user_id, entity_type, entity_id, deleted_date) VALUES
                            (@user_id, @entity_type, @entity_id, @deleted_date)",
                      [ [ "user_id", Sql.int userId
                          "entity_type", Sql.int (LanguagePrimitives.EnumToValue EntityType.AutomaticTransaction)
                          "entity_id", Sql.int id
                          "deleted_date", Sql.timestamptz DateTime.UtcNow ] ]

                      $"DELETE FROM {table} WHERE id = @id AND user_id = @user_id",
                      [ [ "id", Sql.int id; "user_id", Sql.int userId ] ] ]

            metric.Finish()
        }
