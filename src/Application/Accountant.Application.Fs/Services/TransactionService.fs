namespace Accountant.Application.Fs.Services

open Accountant.Domain.Models
open Accountant.Application.Fs.Models.Transactions

module TransactionService =
    let mapAll (categories: seq<Transaction>) : seq<TransactionDto> =
        categories
        |> Seq.map (fun x ->
            { Id = x.Id
              FromAccountId = x.FromAccountId
              ToAccountId = x.ToAccountId
              CategoryId = x.CategoryId
              Amount = x.Amount
              FromStocks = x.FromStocks
              ToStocks = x.ToStocks
              Currency = x.Currency
              Description = x.Description
              Date = x.Date
              IsEncrypted = x.IsEncrypted
              EncryptedDescription = x.EncryptedDescription
              Salt = x.Salt
              Nonce = x.Nonce
              Generated = x.Generated
              CreatedDate = x.CreatedDate
              ModifiedDate = x.ModifiedDate })
