namespace Accountant.Persistence

open System
open System.Threading.Tasks
open Npgsql.FSharp
open Models

module CommonRepository =
    [<Literal>]
    let private schema = "accountant"

    let private tables =
        Map
            [ EntityType.Category, "categories"
              EntityType.Account, "accounts"
              EntityType.Transaction, "transactions"
              EntityType.UpcomingExpense, "upcoming_expenses"
              EntityType.Debt, "debts"
              EntityType.AutomaticTransaction, "automatic_transactions" ]

    let getDeletedIds
        (userId: int)
        (fromDate: DateTime)
        (entityType: EntityType)
        connectionString
        : Task<int list> =
        connectionString
        |> Sql.connect
        |> Sql.query
            $"SELECT entity_id FROM {schema}.deleted_entities WHERE user_id = @userId AND entity_type = @entityType AND deleted_date > @fromDate"
        |> Sql.parameters
            [ "userId", Sql.int userId
              "entityType", Sql.int16 (int16 entityType)
              "fromDate", Sql.timestamptz fromDate ]
        |> Sql.executeAsync (fun read -> read.int "entity_id")

    let exists (id: int) (userId: int) (entity: EntityType) connectionString =
        if entity = EntityType.Transaction then
            raise (ArgumentException("Function does not support Transaction entities"))

        let table = tables[entity]

        connectionString
        |> Sql.connect
        |> Sql.query $"SELECT EXISTS (SELECT 1 FROM {schema}.{table} WHERE id = @id AND user_id = @user_id) AS exists"
        |> Sql.parameters [ "id", Sql.int id; "user_id", Sql.int userId ]
        |> Sql.executeRow (fun read -> read.bool "exists")
