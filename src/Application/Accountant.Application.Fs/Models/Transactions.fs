namespace Accountant.Application.Fs.Models

open System

module Transactions =

    type TransactionDto =
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

    type SyncTransaction =
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
