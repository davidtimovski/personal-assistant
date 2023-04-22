namespace Accountant.Persistence.Fs

open System
open System.Threading.Tasks
open Npgsql.FSharp
open ConnectionUtils
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
        (conn: RegularOrTransactionalConn)
        : Task<int list> =
        ConnectionUtils.connect conn
        |> Sql.query
            "SELECT entity_id FROM accountant.deleted_entities WHERE user_id = @userId AND entity_type = @entityType AND deleted_date > @fromDate"
        |> Sql.parameters
            [ "userId", Sql.int userId
              "entityType", Sql.int16 (int16 entityType)
              "fromDate", Sql.date fromDate ]
        |> Sql.executeAsync (fun read -> read.int "entity_id")

    let exists (id: int) (userId: int) (entity: EntityType) (conn: RegularOrTransactionalConn) =
        if entity = EntityType.Transaction then
            raise (ArgumentException("Method does not support Transaction entities"))

        let table = tables[entity]

        ConnectionUtils.connect conn
        |> Sql.query $"SELECT COUNT(*) AS count FROM {schema}.{table} WHERE id = @id AND user_id = @user_id"
        |> Sql.parameters [ "id", Sql.int id; "user_id", Sql.int userId ]
        |> Sql.executeRow (fun read -> (read.int "count") > 0)
