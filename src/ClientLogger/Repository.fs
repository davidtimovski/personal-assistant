module Repository

open System
open Npgsql
open Npgsql.FSharp
open Models

let createError (error : LogError) (connectionString : string) =
    use connection = new NpgsqlConnection(connectionString)
    connection.Open()

    connection
    |> Sql.existingConnection
    |> Sql.query "INSERT INTO client_errors (user_id, message, stack_trace, occurred, created_date) VALUES (@user_id, @message, @stack_trace, @occurred, @created_date)"
    |> Sql.parameters [
        "@user_id", Sql.int error.UserId
        "@message", Sql.string error.Message
        "@stack_trace", Sql.string error.StackTrace
        "@occurred", Sql.timestamptz error.Occurred
        "@created_date", Sql.timestamptz DateTime.UtcNow
    ]
    |> Sql.executeNonQuery
