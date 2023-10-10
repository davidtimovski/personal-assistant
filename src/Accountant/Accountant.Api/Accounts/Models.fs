namespace Accountant.Api.Accounts

open System
open Microsoft.AspNetCore.Http

module Models =

    type AccountDto =
        { Id: int
          Name: string
          IsMain: bool
          Currency: string
          StockPrice: decimal Option
          CreatedDate: DateTime
          ModifiedDate: DateTime }

    type CreateAccountRequest =
        { Name: string
          Currency: string
          StockPrice: decimal Option
          CreatedDate: DateTime
          ModifiedDate: DateTime }

    type UpdateAccountRequest =
        { mutable HttpContext: HttpContext
          Id: int
          Name: string
          Currency: string
          StockPrice: decimal Option
          CreatedDate: DateTime
          ModifiedDate: DateTime }
