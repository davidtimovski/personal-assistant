namespace Accountant.Api.Accounts

open System

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
        { Id: int
          Name: string
          IsMain: bool
          Currency: string
          StockPrice: decimal Option
          CreatedDate: DateTime
          ModifiedDate: DateTime }
