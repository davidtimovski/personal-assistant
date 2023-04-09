namespace Accountant.Persistence.Fs

open System
open System.Linq
open Microsoft.EntityFrameworkCore
open Npgsql.FSharp
open Accountant.Domain.Models
open CommonRepository

module CategoriesRepository =

    let getAll (userId: int) (fromModifiedDate: DateTime) connectionString =
        connectionString
        |> Sql.connect
        |> Sql.query "SELECT * FROM accountant.categories WHERE user_id = @userId AND modified_date > @fromModifiedDate"
        |> Sql.parameters [
            "userId", Sql.int userId
            "fromModifiedDate", Sql.timestamptz fromModifiedDate ]
        |> Sql.executeAsync (fun read ->
            {
                Id = read.int "id"
                UserId = read.int "user_id"
                ParentId = read.intOrNone "parent_id"
                Name = read.string "name"
                Type = enum<CategoryType>(read.int "type")
                GenerateUpcomingExpense = read.bool "generate_upcoming_expense"
                IsTax = read.bool "is_tax"
                CreatedDate = read.dateTime "created_date"
                ModifiedDate = read.dateTime "modified_date"
            })

    let create (category: Category) (ctx: AccountantContext) =
        task {
            let entity: Entities.Category = 
                { Id = category.Id
                  UserId = category.UserId
                  ParentId = 
                      match category.ParentId with
                      | None -> Nullable<_>()
                      | Some x -> Nullable<_>(x)
                  Name = category.Name
                  Type = category.Type
                  GenerateUpcomingExpense = category.GenerateUpcomingExpense
                  IsTax = category.IsTax
                  CreatedDate = category.CreatedDate
                  ModifiedDate = category.ModifiedDate }

            ctx.Categories.Add(entity) |> ignore
            let! _ = ctx.SaveChangesAsync true |> Async.AwaitTask

            return entity.Id
        }

    let update (category: Category) (ctx: AccountantContext) =
        task {
            let dbCategory = ctx.Categories.AsNoTracking().First(fun x -> x.Id = category.Id && x.UserId = category.UserId)

            let entity: Entities.Category = 
                { Id = dbCategory.Id
                  UserId = dbCategory.UserId
                  ParentId = 
                    match category.ParentId with
                    | None -> Nullable<_>()
                    | Some x -> Nullable<_>(x)
                  Name = category.Name
                  Type = category.Type
                  GenerateUpcomingExpense = category.GenerateUpcomingExpense
                  IsTax = category.IsTax
                  CreatedDate = dbCategory.CreatedDate
                  ModifiedDate = category.ModifiedDate }

            ctx.Attach(entity) |> ignore
            ctx.Entry(entity).State <- EntityState.Modified
 
            ctx.SaveChangesAsync true
                |> Async.AwaitTask
                |> ignore
        }

    let delete (id: int) (userId: int) (ctx: AccountantContext) =
        task {
            CommonRepository.addDeletedEntity userId id EntityType.Category ctx

            let entity = ctx.Categories.First(fun x -> x.Id = id && x.UserId = userId)

            ctx.Remove(entity) |> ignore
 
            ctx.SaveChangesAsync true
                |> Async.AwaitTask
                |> ignore
        }
