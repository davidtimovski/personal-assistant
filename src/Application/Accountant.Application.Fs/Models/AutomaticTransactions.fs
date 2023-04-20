namespace Accountant.Application.Fs.Models

open System

module AutomaticTransactions =

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
        { IsDeposit: bool
          CategoryId: int Option
          Amount: decimal
          Currency: string
          Description: string Option
          DayInMonth: int16
          CreatedDate: DateTime
          ModifiedDate: DateTime }

    type UpdateAutomaticTransaction =
        { Id: int
          IsDeposit: bool
          CategoryId: int Option
          Amount: decimal
          Currency: string
          Description: string Option
          DayInMonth: int16
          CreatedDate: DateTime
          ModifiedDate: DateTime }
