namespace Accountant.Application.Fs.Services

open System
open System.Collections.Generic
open Application.Domain.Accountant
open Accountant.Application.Fs.Models.Accounts

module AccountService =
    let mapAll (accounts: IEnumerable<Account>) : seq<AccountDto> =
        accounts
        |> Seq.map (fun x ->
            { Id = x.Id
              Name = x.Name
              IsMain = x.IsMain
              Currency = x.Currency
              StockPrice = x.StockPrice
              CreatedDate = x.CreatedDate
              ModifiedDate = x.ModifiedDate })

    let prepareForCreate (model: CreateAccount) (userId: int) : Account =
        Account(
            Id = 0,
            UserId = userId,
            Name = model.Name.Trim(),
            IsMain = model.IsMain,
            Currency = model.Currency,
            StockPrice = model.StockPrice,
            CreatedDate = model.CreatedDate,
            ModifiedDate = model.ModifiedDate
        )

    let prepareForCreateMain (userId: int) (name: string) : Account =
        let now = DateTime.UtcNow;

        Account(
            Id = 0,
            UserId = userId,
            Name = name.Trim(),
            IsMain = true,
            Currency = "EUR",
            StockPrice = Nullable(),
            CreatedDate = now,
            ModifiedDate = now
        )

    let prepareForUpdate (model: UpdateAccount) (userId: int) : Account =
        Account(
            Id = model.Id,
            UserId = userId,
            Name = model.Name.Trim(),
            IsMain = model.IsMain,
            Currency = model.Currency,
            StockPrice = model.StockPrice,
            CreatedDate = model.CreatedDate,
            ModifiedDate = model.ModifiedDate
        )
