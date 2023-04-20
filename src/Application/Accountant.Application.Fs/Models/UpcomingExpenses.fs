namespace Accountant.Application.Fs.Models

open System

module UpcomingExpenses =

    type UpcomingExpenseDto =
        { Id: int
          CategoryId: int Option
          Amount: decimal
          Currency: string
          Description: string Option
          Date: DateTime
          Generated: bool
          CreatedDate: DateTime
          ModifiedDate: DateTime }

    type CreateUpcomingExpense =
        { CategoryId: int Option
          Amount: decimal
          Currency: string
          Description: string Option
          Date: DateTime
          Generated: bool
          CreatedDate: DateTime
          ModifiedDate: DateTime }

    type UpdateUpcomingExpense =
        { Id: int
          CategoryId: int Option
          Amount: decimal
          Currency: string
          Description: string Option
          Date: DateTime
          Generated: bool
          CreatedDate: DateTime
          ModifiedDate: DateTime }
