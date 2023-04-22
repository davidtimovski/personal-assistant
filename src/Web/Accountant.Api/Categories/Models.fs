namespace Accountant.Api.Categories

open System
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

    type CreateCategory =
        { ParentId: int Option
          Name: string
          Type: CategoryType
          GenerateUpcomingExpense: bool
          IsTax: bool
          CreatedDate: DateTime
          ModifiedDate: DateTime }

    type UpdateCategory =
        { Id: int
          ParentId: int Option
          Name: string
          Type: CategoryType
          GenerateUpcomingExpense: bool
          IsTax: bool
          CreatedDate: DateTime
          ModifiedDate: DateTime }
