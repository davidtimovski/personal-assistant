namespace Accountant.Application.Fs.Models

open System

module Accounts =

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
          StockPrice: Nullable<decimal>
          CreatedDate: DateTime
          ModifiedDate: DateTime }

    type UpdateAccount =
        { Id: int
          Name: string
          IsMain: bool
          Currency: string
          StockPrice: Nullable<decimal>
          CreatedDate: DateTime
          ModifiedDate: DateTime }

    type SyncAccount =
        { Id: int
          Name: string
          IsMain: bool
          Currency: string
          StockPrice: Nullable<decimal>
          CreatedDate: DateTime
          ModifiedDate: DateTime }
