namespace Accountant.Application.Fs.Services

open Accountant.Domain.Models
open Accountant.Application.Fs.Models.Debts

module DebtService =
    let mapAll (debts: seq<Debt>) : seq<DebtDto> =
        debts
        |> Seq.map (fun x ->
            { Id = x.Id
              Person = x.Person
              Amount = x.Amount
              Currency = x.Currency
              UserIsDebtor = x.UserIsDebtor
              Description = x.Description
              CreatedDate = x.CreatedDate
              ModifiedDate = x.ModifiedDate })

    let prepareForCreate (model: CreateDebt) (userId: int) : Application.Domain.Accountant.Debt =
        let trimmedDesc = (match model.Description with
                            | null -> null
                            | a -> a.Trim())
        
        Application.Domain.Accountant.Debt(
            Id = 0,
            UserId = userId,
            Person = model.Person.Trim(),
            Amount = model.Amount,
            Currency = model.Currency,
            UserIsDebtor = model.UserIsDebtor,
            Description = trimmedDesc,
            CreatedDate = model.CreatedDate,
            ModifiedDate = model.ModifiedDate
        )

    let prepareForCreateMerged (model: CreateDebt) (userId: int) : Application.Domain.Accountant.Debt =
        let trimmedDesc = (match model.Description with
                            | null -> null
                            | a -> a.Trim())
        
        Application.Domain.Accountant.Debt(
            Id = 0,
            UserId = userId,
            Person = model.Person.Trim(),
            Amount = model.Amount,
            Currency = model.Currency,
            UserIsDebtor = model.UserIsDebtor,
            Description = trimmedDesc,
            CreatedDate = model.CreatedDate,
            ModifiedDate = model.ModifiedDate
        )

    let prepareForUpdate (model: UpdateDebt) (userId: int) : Application.Domain.Accountant.Debt =
        let trimmedDesc = (match model.Description with
                            | null -> null
                            | a -> a.Trim())
        
        Application.Domain.Accountant.Debt(
            Id = model.Id,
            UserId = userId,
            Person = model.Person.Trim(),
            Amount = model.Amount,
            Currency = model.Currency,
            UserIsDebtor = model.UserIsDebtor,
            Description = trimmedDesc,
            CreatedDate = model.CreatedDate,
            ModifiedDate = model.ModifiedDate
        )
