namespace Accountant.Persistence.Fs

open System
open Npgsql.FSharp
open ConnectionUtils
open Models
open Npgsql

module TransactionsRepository =

    [<Literal>]
    let private table = "accountant.transactions"

    let getAll (userId: int) (fromModifiedDate: DateTime) (conn: RegularOrTransactionalConn) =
        ConnectionUtils.connect conn
        |> Sql.query
            $"SELECT t.* 
                      FROM {table} AS t
                      INNER JOIN accountant.accounts AS a ON a.id = t.from_account_id 
                          OR a.id = t.to_account_id 
                      WHERE a.user_id = @user_id AND t.modified_date > @modified_date"
        |> Sql.parameters [ "user_id", Sql.int userId; "modified_date", Sql.timestamptz fromModifiedDate ]
        |> Sql.executeAsync (fun read ->
            { Id = read.int "id"
              FromAccountId = read.intOrNone "from_account_id"
              ToAccountId = read.intOrNone "to_account_id"
              CategoryId = read.intOrNone "category_id"
              Amount = read.decimal "amount"
              FromStocks = read.decimalOrNone "from_stocks"
              ToStocks = read.decimalOrNone "to_stocks"
              Currency = read.string "currency"
              Description = read.stringOrNone "description"
              Date = read.dateTime "date"
              IsEncrypted = read.bool "is_encrypted"
              EncryptedDescription = read.byteaOrNone "encrypted_description"
              Salt = read.byteaOrNone "salt"
              Nonce = read.byteaOrNone "nonce"
              Generated = read.bool "generated"
              CreatedDate = read.dateTime "created_date"
              ModifiedDate = read.dateTime "modified_date" })

    let exists (id: int) (userId: int) (conn: RegularOrTransactionalConn) =
        ConnectionUtils.connect conn
        |> Sql.query $"SELECT COUNT(*) AS count
                       FROM {table} AS t
                       LEFT JOIN accountant.accounts AS fa ON t.from_account_id = fa.id
                       LEFT JOIN accountant.accounts AS ta ON t.to_account_id = ta.id
                       WHERE t.id = @id AND (t.from_account_id IS NULL OR fa.user_id = @user_id) AND (t.to_account_id IS NULL OR ta.user_id = @user_id)"
        |> Sql.parameters [ "id", Sql.int id; "user_id", Sql.int userId ]
        |> Sql.executeRow (fun read -> (read.int "count") > 0)

    let create (transaction: Transaction) (conn: RegularOrTransactionalConn) (tran: NpgsqlTransaction Option) =
        let npgsqlConn =
            match conn with
            | ConnectionString cs ->
                let conn = new NpgsqlConnection(cs)
                conn.Open()
                conn
            | TransactionConnection tc -> tc

        // If there is an outside transaction use that one
        // Otherwise begin a new one
        let tr =
            match tran with
            | Some t -> t
            | None -> npgsqlConn.BeginTransaction()

        task {
            let! id =
                npgsqlConn
                |> Sql.existingConnection
                |> Sql.query
                    $"INSERT INTO {table}
                            (from_account_id, to_account_id, category_id, amount, from_stocks, to_stocks, currency, description, date, is_encrypted, encrypted_description, salt, nonce, generated, created_date, modified_date) VALUES 
                            (@from_account_id, @to_account_id, @category_id, @amount, @from_stocks, @to_stocks, @currency, @description, @date, @is_encrypted, @encrypted_description, @salt, @nonce, @generated, @created_date, @modified_date) RETURNING id"
                |> Sql.parameters
                    [ "from_account_id", Sql.intOrNone transaction.FromAccountId
                      "to_account_id", Sql.intOrNone transaction.ToAccountId
                      "category_id", Sql.intOrNone transaction.CategoryId
                      "amount", Sql.decimal transaction.Amount
                      "from_stocks", Sql.decimalOrNone transaction.FromStocks
                      "to_stocks", Sql.decimalOrNone transaction.ToStocks
                      "currency", Sql.string transaction.Currency
                      "description", Sql.stringOrNone transaction.Description
                      "date", Sql.date (transaction.Date.ToUniversalTime())
                      "is_encrypted", Sql.bool transaction.IsEncrypted
                      "encrypted_description", Sql.byteaOrNone transaction.EncryptedDescription
                      "salt", Sql.byteaOrNone transaction.Salt
                      "nonce", Sql.byteaOrNone transaction.Nonce
                      "generated", Sql.bool transaction.Generated
                      "created_date", Sql.timestamptz transaction.CreatedDate
                      "modified_date", Sql.timestamptz transaction.ModifiedDate ]
                |> Sql.executeRowAsync (fun read -> read.int "id")
                |> Async.AwaitTask

            if transaction.FromAccountId.IsSome && transaction.ToAccountId.IsNone then
                let relatedUpcomingExpenses =
                    npgsqlConn
                    |> Sql.existingConnection
                    |> Sql.query
                        "SELECT * FROM accountant.upcoming_expenses
                            WHERE category_id = @category_id 
                                AND EXTRACT(year FROM date) = @year
                                AND EXTRACT(month FROM date) = @month"
                    |> Sql.parameters
                        [ "category_id", Sql.intOrNone transaction.CategoryId
                          "year", Sql.int (transaction.Date.Year)
                          "month", Sql.int (transaction.Date.Month) ]
                    |> Sql.execute (fun read ->
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

                for ue in relatedUpcomingExpenses do
                    let bothWithDescriptionsAndTheyMatch =
                        ue.Description.IsSome
                        && transaction.Description.IsSome
                        && String.Equals(
                            ue.Description.Value,
                            transaction.Description.Value,
                            StringComparison.OrdinalIgnoreCase
                        )

                    if ue.Description.IsNone || bothWithDescriptionsAndTheyMatch then
                        if ue.Amount > transaction.Amount then
                            npgsqlConn
                            |> Sql.existingConnection
                            |> Sql.query
                                "UPDATE accountant.upcoming_expenses
                                    SET amount = amount - @Amount, modified_date = @ModifiedDate
                                    WHERE id = @Id"
                            |> Sql.parameters
                                [ "id", Sql.int ue.Id
                                  "amount", Sql.decimal transaction.Amount
                                  "modified_date", Sql.timestamptz DateTime.UtcNow ]
                            |> Sql.executeNonQueryAsync
                            |> Async.AwaitTask
                            |> ignore
                        else
                            npgsqlConn
                            |> Sql.existingConnection
                            |> Sql.executeTransactionAsync
                                [ "INSERT INTO accountant.deleted_entities
                                      (user_id, entity_type, entity_id, deleted_date) VALUES
                                      (@user_id, @entity_type, @entity_id, @deleted_date)",
                                  [ [ "user_id", Sql.int ue.UserId
                                      "entity_type", Sql.int (LanguagePrimitives.EnumToValue EntityType.UpcomingExpense)
                                      "entity_id", Sql.int id
                                      "deleted_date", Sql.timestamptz DateTime.UtcNow ] ]

                                  $"DELETE FROM {table} WHERE id = @id AND user_id = @user_id",
                                  [ [ "id", Sql.int id; "user_id", Sql.int ue.UserId ] ] ]
                            |> Async.AwaitTask
                            |> ignore

            if tran.IsNone then
                tr.Commit()

            return id
        }

    let update (transaction: Transaction) (conn: RegularOrTransactionalConn) =
        task {
            return!
                ConnectionUtils.connect conn
                |> Sql.query
                    $"UPDATE {table}
                           SET from_account_id = @from_account_id, to_account_id = @to_account_id, category_id = @category_id, amount = @amount, from_stocks = @from_stocks,
                                to_stocks = @to_stocks, currency = @currency, description = @description, date = @date, is_encrypted = @is_encrypted,
                                encrypted_description = @encrypted_description, salt = @salt, nonce = @nonce, modified_date = @modified_date
                           WHERE id = @id"
                |> Sql.parameters
                    [ "id", Sql.int transaction.Id
                      "from_account_id", Sql.intOrNone transaction.FromAccountId
                      "to_account_id", Sql.intOrNone transaction.ToAccountId
                      "category_id", Sql.intOrNone transaction.CategoryId
                      "amount", Sql.decimal transaction.Amount
                      "from_stocks", Sql.decimalOrNone transaction.FromStocks
                      "to_stocks", Sql.decimalOrNone transaction.ToStocks
                      "currency", Sql.string transaction.Currency
                      "description", Sql.stringOrNone transaction.Description
                      "date", Sql.date (transaction.Date.ToUniversalTime())
                      "is_encrypted", Sql.bool transaction.IsEncrypted
                      "encrypted_description", Sql.byteaOrNone transaction.EncryptedDescription
                      "salt", Sql.byteaOrNone transaction.Salt
                      "nonce", Sql.byteaOrNone transaction.Nonce
                      "modified_date", Sql.timestamptz transaction.ModifiedDate ]
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
                      "entity_type", Sql.int (LanguagePrimitives.EnumToValue EntityType.Transaction)
                      "entity_id", Sql.int id
                      "deleted_date", Sql.timestamptz DateTime.UtcNow ] ]

                  $"DELETE FROM accountant.transactions t
                        USING accountant.accounts a
                    WHERE t.id = @id AND 
                        ((t.from_account_id = a.id AND a.user_id = @user_id) OR (t.to_account_id = a.id AND a.user_id = @user_id))",
                  [ [ "id", Sql.int id; "user_id", Sql.int userId ] ] ]
            |> Async.AwaitTask
            |> ignore
        }
