namespace Accountant.Api.Categories

open System
open Microsoft.AspNetCore.Http
open Accountant.Persistence.Fs.Models

module Models =

    type CategoryDto =
        { Id: int
          ParentId: int Option
          Name: string
          Type: CategoryType
          GenerateUpcomingExpense: bool
          IsTax: bool
          CreatedDate: DateTime
          ModifiedDate: DateTime }

    type CreateCategoryRequest =
        { mutable HttpContext: HttpContext
          ParentId: int Option
          Name: string
          Type: CategoryType
          GenerateUpcomingExpense: bool
          IsTax: bool
          CreatedDate: DateTime
          ModifiedDate: DateTime }

    type UpdateCategoryRequest =
        { mutable HttpContext: HttpContext
          Id: int
          ParentId: int Option
          Name: string
          Type: CategoryType
          GenerateUpcomingExpense: bool
          IsTax: bool
          CreatedDate: DateTime
          ModifiedDate: DateTime }
