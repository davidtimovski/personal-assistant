namespace Accountant.Application.Fs.Services

open System
open System.Collections.Generic
open Domain.Accountant
open Accountant.Application.Fs.Models.UpcomingExpenses

module UpcomingExpenseService =
    let mapAll (categories: IEnumerable<UpcomingExpense>) : seq<UpcomingExpenseDto> =
        categories
        |> Seq.map (fun x ->
            { Id = x.Id
              CategoryId = x.CategoryId
              Amount = x.Amount
              Currency = x.Currency
              Description = x.Description
              Date = x.Date
              Generated = x.Generated
              CreatedDate = x.CreatedDate
              ModifiedDate = x.ModifiedDate })

    let prepareForCreate (model: CreateUpcomingExpense) (userId: int) : UpcomingExpense =
        let trimmedDesc = (match model.Description with
                            | null -> null
                            | a -> a.Trim())
        
        UpcomingExpense(
            Id = 0,
            UserId = userId,
            CategoryId = model.CategoryId,
            Amount = model.Amount,
            Currency = model.Currency,
            Description = trimmedDesc,
            Date = model.Date,
            Generated = model.Generated,
            CreatedDate = model.CreatedDate,
            ModifiedDate = model.ModifiedDate
        )

    let prepareForUpdate (model: UpdateUpcomingExpense) (userId: int) : UpcomingExpense =
        let trimmedDesc = (match model.Description with
                            | null -> null
                            | a -> a.Trim())
        
        UpcomingExpense(
            Id = model.Id,
            UserId = userId,
            CategoryId = model.CategoryId,
            Amount = model.Amount,
            Currency = model.Currency,
            Description = trimmedDesc,
            Date = model.Date,
            Generated = model.Generated,
            CreatedDate = model.CreatedDate,
            ModifiedDate = model.ModifiedDate
        )

    let getFirstDayOfMonth =
        new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0)
