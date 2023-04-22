namespace Accountant.Api.Accounts

open System
open Accountant.Persistence.Fs
open Models

module Logic =
    let mapAll (accounts: seq<Account>) : seq<AccountDto> =
        accounts
        |> Seq.map (fun x ->
            { Id = x.Id
              Name = x.Name
              IsMain = x.IsMain
              Currency = x.Currency
              StockPrice = x.StockPrice
              CreatedDate = x.CreatedDate
              ModifiedDate = x.ModifiedDate })

    let prepareForCreate (model: CreateAccount) (userId: int) =
        { Id = 0
          UserId = userId
          Name = model.Name.Trim()
          IsMain = model.IsMain
          Currency = model.Currency
          StockPrice = model.StockPrice
          CreatedDate = model.CreatedDate
          ModifiedDate = model.ModifiedDate }

    let prepareForCreateMain (userId: int) (name: string) =
        let now = DateTime.UtcNow

        { Id = 0
          UserId = userId
          Name = name.Trim()
          IsMain = true
          Currency = "EUR"
          StockPrice = None
          CreatedDate = now
          ModifiedDate = now }

    let prepareForUpdate (model: UpdateAccount) (userId: int) =
        { Id = model.Id
          UserId = userId
          Name = model.Name.Trim()
          IsMain = model.IsMain
          Currency = model.Currency
          StockPrice = model.StockPrice
          CreatedDate = model.CreatedDate
          ModifiedDate = model.ModifiedDate }
