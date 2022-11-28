namespace Accountant.Application.Fs.Models

open System

module AutomaticTransactions =

    type AutomaticTransactionDto =
        { Id: int
          IsDeposit: bool
          CategoryId: Nullable<int>
          Amount: decimal
          Currency: string
          Description: string
          DayInMonth: int16
          CreatedDate: DateTime
          ModifiedDate: DateTime }

    type CreateAutomaticTransaction =
        { IsDeposit: bool
          CategoryId: Nullable<int>
          Amount: decimal
          Currency: string
          Description: string
          DayInMonth: int16
          CreatedDate: DateTime
          ModifiedDate: DateTime }

    type UpdateAutomaticTransaction =
        { Id: int
          IsDeposit: bool
          CategoryId: Nullable<int>
          Amount: decimal
          Currency: string
          Description: string
          DayInMonth: int16
          CreatedDate: DateTime
          ModifiedDate: DateTime }
