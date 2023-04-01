﻿namespace Accountant.Application.Fs.Services

open System
open Accountant.Domain.Models
open Accountant.Application.Fs.Models.AutomaticTransactions

module AutomaticTransactionService =
    let mapAll (automaticTransactions: seq<AutomaticTransaction>) : seq<AutomaticTransactionDto> =
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

    let prepareForCreate (model: CreateAutomaticTransaction) (userId: int) : Application.Domain.Accountant.AutomaticTransaction =
        let trimmedDesc = (match model.Description with
                            | null -> null
                            | a -> a.Trim())
        
        Application.Domain.Accountant.AutomaticTransaction(
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

    let prepareForUpdate (model: UpdateAutomaticTransaction) (userId: int) : Application.Domain.Accountant.AutomaticTransaction =
        let trimmedDesc = (match model.Description with
                            | null -> null
                            | a -> a.Trim())
        
        Application.Domain.Accountant.AutomaticTransaction(
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
