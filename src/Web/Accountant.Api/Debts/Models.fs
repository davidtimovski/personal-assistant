namespace Accountant.Api.Debts

open System

module Models =

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
          Description: string Option
          CreatedDate: DateTime
          ModifiedDate: DateTime }

    type UpdateDebt =
        { Id: int
          Person: string
          Amount: decimal
          Currency: string
          UserIsDebtor: bool
          Description: string Option
          CreatedDate: DateTime
          ModifiedDate: DateTime }
