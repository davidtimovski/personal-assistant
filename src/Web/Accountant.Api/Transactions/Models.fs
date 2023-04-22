namespace Accountant.Api.Transactions

open System
open Microsoft.AspNetCore.Http

module Models =

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

    type CreateTransaction =
        { mutable HttpContext: HttpContext
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
          CreatedDate: DateTime
          ModifiedDate: DateTime }

    type UpdateTransaction =
        { mutable HttpContext: HttpContext
          Id: int
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
          CreatedDate: DateTime
          ModifiedDate: DateTime }

    type ExportDto = { FileId: Guid }
