namespace Accountant.Persistence.Fs

open System
open Npgsql.FSharp
open ConnectionUtils
open Models

module AccountsRepository =

    [<Literal>]
    let private table = "accountant.accounts"

    let getAll (userId: int) (fromModifiedDate: DateTime) connectionString =
        connectionString
        |> Sql.connect
        |> Sql.query $"SELECT * FROM {table} WHERE user_id = @user_id AND modified_date > @modified_date"
        |> Sql.parameters [ "user_id", Sql.int userId; "modified_date", Sql.timestamptz fromModifiedDate ]
        |> Sql.executeAsync (fun read ->
            { Id = read.int "id"
              UserId = read.int "user_id"
              Name = read.string "name"
              IsMain = read.bool "is_main"
              Currency = read.string "currency"
              StockPrice = read.decimalOrNone "stock_price"
              CreatedDate = read.dateTime "created_date"
              ModifiedDate = read.dateTime "modified_date" })

    let hasMain (userId: int) connectionString =
        connectionString
        |> Sql.connect
        |> Sql.query $"SELECT COUNT(*) AS count FROM {table} WHERE user_id = @user_id AND is_main"
        |> Sql.parameters [ "user_id", Sql.int userId ]
        |> Sql.executeRow (fun read -> (read.int "count") > 0)

    let isMain (id: int) (userId: int) connectionString =
        connectionString
        |> Sql.connect
        |> Sql.query $"SELECT COUNT(*) AS count FROM {table} WHERE id = @id AND user_id = @user_id AND is_main"
        |> Sql.parameters [ "id", Sql.int id; "user_id", Sql.int userId ]
        |> Sql.executeRow (fun read -> (read.int "count") > 0)

    let create (account: Account) (conn: RegularOrTransactionalConn) =
        ConnectionUtils.connect conn
        |> Sql.query
            $"INSERT INTO {table}
                  (user_id, name, is_main, currency, stock_price, created_date, modified_date) VALUES 
                  (@user_id, @name, @is_main, @currency, @stock_price, @created_date, @modified_date) RETURNING id"
        |> Sql.parameters
            [ "user_id", Sql.int account.UserId
              "name", Sql.string account.Name
              "is_main", Sql.bool account.IsMain
              "currency", Sql.string account.Currency
              "stock_price", Sql.decimalOrNone account.StockPrice
              "created_date", Sql.timestamptz account.CreatedDate
              "modified_date", Sql.timestamptz account.ModifiedDate ]
        |> Sql.executeRowAsync (fun read -> read.int "id")

    // TODO: Only for Account project
    let createMain (account: Account) connectionString =
        connectionString
        |> Sql.connect
        |> Sql.query
            $"INSERT INTO {table}
                  (user_id, name, is_main, currency, stock_price, created_date, modified_date) VALUES 
                  (@user_id, @name, @is_main, @currency, @stock_price, @created_date, @modified_date) RETURNING id"
        |> Sql.parameters
            [ "user_id", Sql.int account.UserId
              "name", Sql.string account.Name
              "is_main", Sql.bool account.IsMain
              "currency", Sql.string account.Currency
              "stock_price", Sql.decimalOrNone account.StockPrice
              "created_date", Sql.timestamptz account.CreatedDate
              "modified_date", Sql.timestamptz account.ModifiedDate ]
        |> Sql.executeRowAsync (fun read -> read.int "id")

    let update (account: Account) connectionString =
        connectionString
        |> Sql.connect
        |> Sql.query
            $"UPDATE {table}
              SET name = @name, currency = @currency, stock_price = @stock_price, modified_date = @modified_date
              WHERE id = @id AND user_id = @user_id"
        |> Sql.parameters
            [ "id", Sql.int account.Id
              "user_id", Sql.int account.UserId
              "name", Sql.string account.Name
              "currency", Sql.string account.Currency
              "stock_price", Sql.decimalOrNone account.StockPrice
              "modified_date", Sql.timestamptz account.ModifiedDate ]
        |> Sql.executeNonQueryAsync

    let delete (id: int) (userId: int) connectionString =
        connectionString
        |> Sql.connect
        |> Sql.executeTransactionAsync
            [ "INSERT INTO accountant.deleted_entities
                    (user_id, entity_type, entity_id, deleted_date) VALUES
                    (@user_id, @entity_type, @entity_id, @deleted_date)",
              [ [ "user_id", Sql.int userId
                  "entity_type", Sql.int (LanguagePrimitives.EnumToValue EntityType.Account)
                  "entity_id", Sql.int id
                  "deleted_date", Sql.timestamptz DateTime.UtcNow ] ]

              $"DELETE FROM {table} WHERE id = @id AND user_id = @user_id",
              [ [ "id", Sql.int id; "user_id", Sql.int userId ] ] ]
