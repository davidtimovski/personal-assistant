namespace Accountant.Persistence.Fs

open System
open Npgsql.FSharp
open Accountant.Domain.Models

module TransactionsRepository =

    let getAll (userId: int) (fromModifiedDate: DateTime) connectionString =
        connectionString
        |> Sql.connect
        |> Sql.query "SELECT t.* 
                      FROM accountant.transactions AS t
                      INNER JOIN accountant.accounts AS a ON a.id = t.from_account_id 
                          OR a.id = t.to_account_id 
                      WHERE a.user_id = @userId AND t.modified_date > @fromModifiedDate"
        |> Sql.parameters [
            "userId", Sql.int userId
            "fromModifiedDate", Sql.timestamptz fromModifiedDate ]
        |> Sql.executeAsync (fun read ->
            {
                Id = read.int "id"
                FromAccountId = read.intOrNone "from_account_id"
                ToAccountId = read.intOrNone "to_account_id"
                CategoryId = read.intOrNone "category_id"
                Amount = read.decimal "amount"
                FromStocks = read.decimalOrNone "from_stocks"
                ToStocks = read.decimalOrNone "to_stocks"
                Currency = read.string "currency"
                Description = read.stringOrNone "description"
                Date = read.dateTime "date"
                IsEncrypted = read.bool "is_encrypted"
                EncryptedDescription = read.byteaOrNone "encrypted_description"
                Salt = read.byteaOrNone "salt"
                Nonce = read.byteaOrNone "nonce"
                Generated = read.bool "generated"
                CreatedDate = read.dateTime "created_date"
                ModifiedDate = read.dateTime "modified_date"
            })
