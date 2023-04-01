namespace Accountant.Application.Fs.Services

open System
open Accountant.Domain.Models
open Accountant.Application.Fs.Models.Accounts

module AccountService =
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

    let prepareForCreate (model: CreateAccount) (userId: int) : Application.Domain.Accountant.Account =
        Application.Domain.Accountant.Account(
            Id = 0,
            UserId = userId,
            Name = model.Name.Trim(),
            IsMain = model.IsMain,
            Currency = model.Currency,
            StockPrice = model.StockPrice,
            CreatedDate = model.CreatedDate,
            ModifiedDate = model.ModifiedDate
        )

    let prepareForCreateMain (userId: int) (name: string) : Application.Domain.Accountant.Account =
        let now = DateTime.UtcNow;

        Application.Domain.Accountant.Account(
            Id = 0,
            UserId = userId,
            Name = name.Trim(),
            IsMain = true,
            Currency = "EUR",
            StockPrice = Nullable(),
            CreatedDate = now,
            ModifiedDate = now
        )

    let prepareForUpdate (model: UpdateAccount) (userId: int) : Application.Domain.Accountant.Account =
        Application.Domain.Accountant.Account(
            Id = model.Id,
            UserId = userId,
            Name = model.Name.Trim(),
            IsMain = model.IsMain,
            Currency = model.Currency,
            StockPrice = model.StockPrice,
            CreatedDate = model.CreatedDate,
            ModifiedDate = model.ModifiedDate
        )
