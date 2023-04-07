namespace Accountant.Persistence.Fs

open System
open Accountant.Domain.Models

module Entities =
    [<CLIMutable>]
    type Account =
        { Id: int
          UserId: int
          Name: string
          IsMain: bool
          Currency: string
          StockPrice: Nullable<decimal>
          CreatedDate: DateTime
          ModifiedDate: DateTime }

    [<CLIMutable>]
    type Transaction =
        { Id: int
          FromAccountId: Nullable<int>
          ToAccountId: Nullable<int>
          CategoryId: Nullable<int>
          Amount: decimal
          FromStocks: Nullable<decimal>
          ToStocks: Nullable<decimal>
          Currency: string
          Description: string
          Date: DateTime
          IsEncrypted: bool
          EncryptedDescription: byte[]
          Salt: byte[]
          Nonce: byte[]
          Generated: bool
          CreatedDate: DateTime
          ModifiedDate: DateTime }

    [<CLIMutable>]
    type AutomaticTransaction =
        { Id: int
          UserId: int
          IsDeposit: bool
          CategoryId: Nullable<int>
          Amount: decimal
          Currency: string
          Description: string
          DayInMonth: int16
          CreatedDate: DateTime
          ModifiedDate: DateTime }

    [<CLIMutable>]
    type Category =
        { Id: int
          UserId: int
          ParentId: Nullable<int>
          Name: string
          Type: CategoryType
          GenerateUpcomingExpense: bool
          IsTax: bool
          CreatedDate: DateTime
          ModifiedDate: DateTime }

    [<CLIMutable>]
    type Debt =
        { Id: int
          UserId: int
          Person: string
          Amount: decimal
          Currency: string
          UserIsDebtor: bool
          Description: string
          CreatedDate: DateTime
          ModifiedDate: DateTime }

    [<CLIMutable>]
    type UpcomingExpense =
        { Id: int
          UserId: int
          CategoryId: Nullable<int>
          Amount: decimal
          Currency: string
          Description: string
          Date: DateTime
          Generated: bool
          CreatedDate: DateTime
          ModifiedDate: DateTime }

    [<CLIMutable>]
    type DeletedEntity =
        { UserId: int
          EntityType: EntityType
          EntityId: int
          DeletedDate: DateTime }
