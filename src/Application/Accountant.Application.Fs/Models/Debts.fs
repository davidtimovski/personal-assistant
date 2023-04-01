namespace Accountant.Application.Fs.Models

open System

module Debts =

    type DebtDto =
        { Id: int
          Person: string
          Amount: decimal
          Currency: string
          UserIsDebtor: bool
          Description: string Option
          CreatedDate: DateTime
          ModifiedDate: DateTime }

    type CreateDebt =
        { Person: string
          Amount: decimal
          Currency: string
          UserIsDebtor: bool
          Description: string
          CreatedDate: DateTime
          ModifiedDate: DateTime }

    type UpdateDebt =
        { Id: int
          Person: string
          Amount: decimal
          Currency: string
          UserIsDebtor: bool
          Description: string
          CreatedDate: DateTime
          ModifiedDate: DateTime }

    type SyncDebt =
        { Id: int
          Person: string
          Amount: decimal
          Currency: string
          UserIsDebtor: bool
          Description: string
          CreatedDate: DateTime
          ModifiedDate: DateTime }
