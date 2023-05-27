namespace Accountant.Api.AutomaticTransactions

open System
open Microsoft.AspNetCore.Http

module Models =

    type AutomaticTransactionDto =
        { Id: int
          IsDeposit: bool
          CategoryId: int Option
          Amount: decimal
          Currency: string
          Description: string Option
          DayInMonth: int16
          CreatedDate: DateTime
          ModifiedDate: DateTime }

    type CreateAutomaticTransaction =
        { mutable HttpContext: HttpContext
          IsDeposit: bool
          CategoryId: int Option
          Amount: decimal
          Currency: string
          Description: string Option
          DayInMonth: int16
          CreatedDate: DateTime
          ModifiedDate: DateTime }

    type UpdateAutomaticTransaction =
        { mutable HttpContext: HttpContext
          Id: int
          IsDeposit: bool
          CategoryId: int Option
          Amount: decimal
          Currency: string
          Description: string Option
          DayInMonth: int16
          CreatedDate: DateTime
          ModifiedDate: DateTime }
