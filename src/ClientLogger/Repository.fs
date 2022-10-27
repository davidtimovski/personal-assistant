module Repository

open System
open Npgsql
open Npgsql.FSharp
open Models

let createError (model : CreateError) (connectionString : string) =
    use connection = new NpgsqlConnection(connectionString)
    connection.Open()

    connection
    |> Sql.existingConnection
    |> Sql.query "INSERT INTO client_errors (user_id, application, message, stack_trace, occurred, created_date) VALUES (@user_id, @application, @message, @stack_trace, @occurred, @created_date)"
    |> Sql.parameters [
        "@user_id", Sql.int model.UserId
        "@application", Sql.string model.Application
        "@message", Sql.string model.Message
        "@stack_trace", Sql.string model.StackTrace
        "@occurred", Sql.timestamptz (model.Occurred.ToUniversalTime())
        "@created_date", Sql.timestamptz DateTime.UtcNow
    ]
    |> Sql.executeNonQuery
