namespace Accountant.Application.Fs.Models

open System

module UpcomingExpenses =

    type UpcomingExpenseDto =
        { Id: int
          CategoryId: Nullable<int>
          Amount: decimal
          Currency: string
          Description: string
          Date: DateTime
          Generated: bool
          CreatedDate: DateTime
          ModifiedDate: DateTime }

    type CreateUpcomingExpense =
        { CategoryId: Nullable<int>
          Amount: decimal
          Currency: string
          Description: string
          Date: DateTime
          Generated: bool
          CreatedDate: DateTime
          ModifiedDate: DateTime }

    type UpdateUpcomingExpense =
        { Id: int
          CategoryId: Nullable<int>
          Amount: decimal
          Currency: string
          Description: string
          Date: DateTime
          Generated: bool
          CreatedDate: DateTime
          ModifiedDate: DateTime }
