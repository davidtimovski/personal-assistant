namespace Accountant.Api.Debts

open System
open Microsoft.AspNetCore.Http

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

    type CreateDebtRequest =
        { Person: string
          Amount: decimal
          Currency: string
          Description: string Option
          UserIsDebtor: bool
          CreatedDate: DateTime
          ModifiedDate: DateTime }

    type UpdateDebtRequest =
        { mutable HttpContext: HttpContext
          Id: int
          Person: string
          Amount: decimal
          Currency: string
          Description: string Option
          UserIsDebtor: bool
          CreatedDate: DateTime
          ModifiedDate: DateTime }
