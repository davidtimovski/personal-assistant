namespace Accountant.Api.UpcomingExpenses

open System
open Microsoft.AspNetCore.Http

module Models =

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
        { mutable HttpContext: HttpContext
          CategoryId: int Option
          Amount: decimal
          Currency: string
          Description: string Option
          Date: DateTime
          Generated: bool
          CreatedDate: DateTime
          ModifiedDate: DateTime }

    type UpdateUpcomingExpense =
        { mutable HttpContext: HttpContext
          Id: int
          CategoryId: int Option
          Amount: decimal
          Currency: string
          Description: string Option
          Date: DateTime
          Generated: bool
          CreatedDate: DateTime
          ModifiedDate: DateTime }
