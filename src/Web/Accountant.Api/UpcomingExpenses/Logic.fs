namespace Accountant.Api.UpcomingExpenses

open System
open Accountant.Persistence.Fs
open Accountant.Api.HandlerBase
open CommonHandlers
open Models

module Logic =
    [<Literal>]
    let private descriptionMaxLength = 250

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

    let private validateCreateCategory (dto: CreateUpcomingExpense) =
        let userId = getUserId dto.HttpContext
        let connection = getDbConnection dto.HttpContext

        if
            dto.CategoryId.IsNone
            || Validation.categoryBelongsTo dto.CategoryId.Value userId connection
        then
            Success dto
        else
            Failure "Category is not valid"

    let private validateCreateAmount (dto: CreateUpcomingExpense) =
        if Validation.amountIsValid dto.Amount then
            Success dto
        else
            Failure "Amount has to be a positive number"

    let private validateCreateCurrency (dto: CreateUpcomingExpense) =
        if Validation.currencyIsValid dto.Currency then
            Success dto
        else
            Failure "Currency is not valid"

    let private validateCreateDescription (dto: CreateUpcomingExpense) =
        if Validation.textIsNoneOrLengthIsValid dto.Description descriptionMaxLength then
            Success dto
        else
            Failure $"Description cannot exceed {descriptionMaxLength} characters"

    let validateCreate =
        validateCreateCategory
        >> bind validateCreateAmount
        >> bind validateCreateCurrency
        >> bind validateCreateDescription

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

    let private validateUpdateUpcomingExpense (dto: UpdateUpcomingExpense) =
        let userId = getUserId dto.HttpContext
        let connection = getDbConnection dto.HttpContext

        if Validation.upcomingExpenseBelongsTo dto.Id userId connection then
            Success dto
        else
            Failure "Upcoming expense is not valid"

    let private validateUpdateCategory (dto: UpdateUpcomingExpense) =
        let userId = getUserId dto.HttpContext
        let connection = getDbConnection dto.HttpContext

        if
            dto.CategoryId.IsNone
            || Validation.categoryBelongsTo dto.CategoryId.Value userId connection
        then
            Success dto
        else
            Failure "Category is not valid"

    let private validateUpdateAmount (dto: UpdateUpcomingExpense) =
        if Validation.amountIsValid dto.Amount then
            Success dto
        else
            Failure "Amount has to be a positive number"

    let private validateUpdateCurrency (dto: UpdateUpcomingExpense) =
        if Validation.currencyIsValid dto.Currency then
            Success dto
        else
            Failure "Currency is not valid"

    let private validateUpdateDescription (dto: UpdateUpcomingExpense) =
        if Validation.textIsNoneOrLengthIsValid dto.Description descriptionMaxLength then
            Success dto
        else
            Failure $"Description cannot exceed {descriptionMaxLength} characters"

    let validateUpdate =
        validateUpdateUpcomingExpense
        >> bind validateUpdateCategory
        >> bind validateUpdateAmount
        >> bind validateUpdateCurrency
        >> bind validateUpdateDescription

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
