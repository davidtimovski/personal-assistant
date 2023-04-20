namespace Accountant.Persistence.Fs

open System
open System.Threading.Tasks
open Npgsql.FSharp
open Accountant.Domain.Models
open ConnectionUtils

module CommonRepository =
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
