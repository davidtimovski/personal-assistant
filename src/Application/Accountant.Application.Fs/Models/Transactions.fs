namespace Accountant.Application.Fs.Models

open System

module Transactions =

    type TransactionDto =
        { Id: int
          FromAccountId: int Option
          ToAccountId: int Option
          CategoryId: int Option
          Amount: decimal
          FromStocks: decimal Option
          ToStocks: decimal Option
          Currency: string
          Description: string Option
          Date: DateTime
          IsEncrypted: bool
          EncryptedDescription: byte[] Option
          Salt: byte[] Option
          Nonce: byte[] Option
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
