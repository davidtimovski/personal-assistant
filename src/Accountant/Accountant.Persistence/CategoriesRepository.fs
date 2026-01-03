namespace Accountant.Persistence

open System
open Npgsql.FSharp
open ConnectionUtils
open Models
open Sentry

module CategoriesRepository =

    [<Literal>]
    let private table = "accountant.categories"

    let private rowToEntity (read: RowReader) =
        { Id = read.int "id"
          UserId = read.int "user_id"
          ParentId = read.intOrNone "parent_id"
          Name = read.string "name"
          Type = enum<CategoryType> (read.int "type")
          GenerateUpcomingExpense = read.bool "generate_upcoming_expense"
          IsTax = read.bool "is_tax"
          CreatedDate = read.dateTime "created_date"
          ModifiedDate = read.dateTime "modified_date" }

    let get (id: int) connectionString =
        task {
            let! categories =
                connectionString
                |> Sql.connect
                |> Sql.query $"SELECT * FROM {table} WHERE id = @id"
                |> Sql.parameters [ "id", Sql.int id ]
                |> Sql.executeAsync rowToEntity
            
            return if categories.Length > 0 then Some categories[0] else None
        }

    let getAll (userId: int) (fromModifiedDate: DateTime) connectionString =
        connectionString
        |> Sql.connect
        |> Sql.query $"SELECT * FROM {table} WHERE user_id = @user_id AND modified_date > @modified_date"
        |> Sql.parameters [ "user_id", Sql.int userId; "modified_date", Sql.timestamptz fromModifiedDate ]
        |> Sql.executeAsync rowToEntity

    let create (category: Category) (conn: RegularOrTransactionalConn) (metricsSpan: ISpan) =
        let metric = metricsSpan.StartChild("CategoriesRepository.create")

        task {
            let! id =
                ConnectionUtils.connect conn
                |> Sql.query
                    $"INSERT INTO {table}
                          (parent_id, user_id, name, type, generate_upcoming_expense, is_tax, created_date, modified_date) VALUES 
                          (@parent_id, @user_id, @name, @type, @generate_upcoming_expense, @is_tax, @created_date, @modified_date) RETURNING id"
                |> Sql.parameters
                    [ "parent_id", Sql.intOrNone category.ParentId
                      "user_id", Sql.int category.UserId
                      "name", Sql.string category.Name
                      "type", Sql.int (LanguagePrimitives.EnumToValue category.Type)
                      "generate_upcoming_expense", Sql.bool category.GenerateUpcomingExpense
                      "is_tax", Sql.bool category.IsTax
                      "created_date", Sql.timestamptz category.CreatedDate
                      "modified_date", Sql.timestamptz category.ModifiedDate ]
                |> Sql.executeRowAsync (fun read -> read.int "id")

            metric.Finish()

            return id
        }

    let update (category: Category) connectionString (metricsSpan: ISpan) =
        let metric = metricsSpan.StartChild("CategoriesRepository.update")

        task {
            let! _ =
                connectionString
                |> Sql.connect
                |> Sql.query
                    $"UPDATE {table}
                        SET parent_id = @parent_id, name = @name, type = @type, generate_upcoming_expense = @generate_upcoming_expense, is_tax = @is_tax, modified_date = @modified_date
                        WHERE id = @id AND user_id = @user_id"
                |> Sql.parameters
                    [ "id", Sql.int category.Id
                      "parent_id", Sql.intOrNone category.ParentId
                      "user_id", Sql.int category.UserId
                      "name", Sql.string category.Name
                      "type", Sql.int (LanguagePrimitives.EnumToValue category.Type)
                      "generate_upcoming_expense", Sql.bool category.GenerateUpcomingExpense
                      "is_tax", Sql.bool category.IsTax
                      "modified_date", Sql.timestamptz category.ModifiedDate ]
                |> Sql.executeNonQueryAsync

            metric.Finish()
        }

    let delete (id: int) (userId: int) connectionString (metricsSpan: ISpan) =
        let metric = metricsSpan.StartChild("CategoriesRepository.delete")

        task {
            let! _ =
                connectionString
                |> Sql.connect
                |> Sql.executeTransactionAsync
                    [ "INSERT INTO accountant.deleted_entities
                              (user_id, entity_type, entity_id, deleted_date) VALUES
                              (@user_id, @entity_type, @entity_id, @deleted_date)",
                      [ [ "user_id", Sql.int userId
                          "entity_type", Sql.int (LanguagePrimitives.EnumToValue EntityType.Category)
                          "entity_id", Sql.int id
                          "deleted_date", Sql.timestamptz DateTime.UtcNow ] ]

                      $"DELETE FROM {table} WHERE id = @id AND user_id = @user_id",
                      [ [ "id", Sql.int id; "user_id", Sql.int userId ] ] ]

            metric.Finish()
        }
