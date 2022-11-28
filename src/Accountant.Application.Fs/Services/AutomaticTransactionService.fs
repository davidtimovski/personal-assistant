namespace Accountant.Application.Fs.Services

open System.Collections.Generic
open Domain.Accountant
open Accountant.Application.Fs.Models.AutomaticTransactions

module AutomaticTransactionService =
    let mapAll (automaticTransactions: IEnumerable<AutomaticTransaction>) : seq<AutomaticTransactionDto> =
        automaticTransactions
        |> Seq.map (fun x ->
            { Id = x.Id
              IsDeposit = x.IsDeposit
              CategoryId = x.CategoryId
              Amount = x.Amount
              Currency = x.Currency
              Description = x.Description
              DayInMonth = x.DayInMonth
              CreatedDate = x.CreatedDate
              ModifiedDate = x.ModifiedDate })

    let prepareForCreate (model: CreateAutomaticTransaction) (userId: int) : AutomaticTransaction =
        let trimmedDesc = (match model.Description with
                            | null -> null
                            | a -> a.Trim())
        
        AutomaticTransaction(
            Id = 0,
            UserId = userId,
            IsDeposit = model.IsDeposit,
            CategoryId = model.CategoryId,
            Amount = model.Amount,
            Currency = model.Currency,
            Description = trimmedDesc,
            DayInMonth = model.DayInMonth,
            CreatedDate = model.CreatedDate,
            ModifiedDate = model.ModifiedDate
        )

    let prepareForUpdate (model: UpdateAutomaticTransaction) (userId: int) : AutomaticTransaction =
        let trimmedDesc = (match model.Description with
                            | null -> null
                            | a -> a.Trim())
        
        AutomaticTransaction(
            Id = model.Id,
            UserId = userId,
            IsDeposit = model.IsDeposit,
            CategoryId = model.CategoryId,
            Amount = model.Amount,
            Currency = model.Currency,
            Description = trimmedDesc,
            DayInMonth = model.DayInMonth,
            CreatedDate = model.CreatedDate,
            ModifiedDate = model.ModifiedDate
        )
