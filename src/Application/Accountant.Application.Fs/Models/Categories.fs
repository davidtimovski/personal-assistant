namespace Accountant.Application.Fs.Models

open System
open Accountant.Domain.Models

module Categories =

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
