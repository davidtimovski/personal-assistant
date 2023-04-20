namespace Accountant.Application.Fs.Services

open System
open Accountant.Domain.Models
open Accountant.Application.Fs
open Accountant.Application.Fs.Models.UpcomingExpenses

module UpcomingExpenseService =
    let mapAll (categories: seq<UpcomingExpense>) : seq<UpcomingExpenseDto> =
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

    let prepareForCreate (model: CreateUpcomingExpense) (userId: int) =
        { Id = 0
          UserId = userId
          CategoryId = model.CategoryId
          Amount = model.Amount
          Currency = model.Currency
          Description = Utils.noneOrTrimmed model.Description
          Date = model.Date
          Generated = model.Generated
          CreatedDate = model.CreatedDate
          ModifiedDate = model.ModifiedDate }

    let prepareForUpdate (model: UpdateUpcomingExpense) (userId: int) =
        { Id = model.Id
          UserId = userId
          CategoryId = model.CategoryId
          Amount = model.Amount
          Currency = model.Currency
          Description = Utils.noneOrTrimmed model.Description
          Date = model.Date
          Generated = model.Generated
          CreatedDate = model.CreatedDate
          ModifiedDate = model.ModifiedDate }

    let getFirstDayOfMonth =
        new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0)
