namespace Accountant.Persistence

open Npgsql
open Npgsql.FSharp

module ConnectionUtils =

    type RegularOrTransactionalConn =
    | ConnectionString of string
    | TransactionConnection of NpgsqlConnection

    let connect (conn: RegularOrTransactionalConn) =
        match conn with
        | ConnectionString connectionString ->
            connectionString
            |> Sql.connect
        | TransactionConnection conn ->
            conn
            |> Sql.existingConnection
