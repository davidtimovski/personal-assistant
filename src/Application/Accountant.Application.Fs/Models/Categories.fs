namespace Accountant.Application.Fs.Models

open System
open Application.Domain.Accountant

module Categories =

    type CategoryDto =
        { Id: int
          ParentId: int Option
          Name: string
          Type: Accountant.Domain.Models.CategoryType
          GenerateUpcomingExpense: bool
          IsTax: bool
          CreatedDate: DateTime
          ModifiedDate: DateTime }

    type CreateCategory =
        { ParentId: Nullable<int>
          Name: string
          Type: CategoryType
          GenerateUpcomingExpense: bool
          IsTax: bool
          CreatedDate: DateTime
          ModifiedDate: DateTime }

    type UpdateCategory =
        { Id: int
          ParentId: Nullable<int>
          Name: string
          Type: CategoryType
          GenerateUpcomingExpense: bool
          IsTax: bool
          CreatedDate: DateTime
          ModifiedDate: DateTime }

    type SyncCategory =
        { Id: int
          ParentId: Nullable<int>
          Name: string
          Type: CategoryType
          GenerateUpcomingExpense: bool
          IsTax: bool
          CreatedDate: DateTime
          ModifiedDate: DateTime }
