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

    type CreateAccount =
        { Name: string
          IsMain: bool
          Currency: string
          StockPrice: decimal Option
          CreatedDate: DateTime
          ModifiedDate: DateTime }

    type UpdateAccount =
        { mutable HttpContext: HttpContext
          Id: int
          Name: string
          IsMain: bool
          Currency: string
          StockPrice: decimal Option
          CreatedDate: DateTime
          ModifiedDate: DateTime }
