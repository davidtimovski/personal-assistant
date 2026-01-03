namespace Accountant.Api.UpcomingExpenses

open System
open Accountant.Persistence
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

    let private validateCreateCategory (request: CreateUpcomingExpenseRequest) =
        let userId = getUserId request.HttpContext
        let connectionString = getConnectionString request.HttpContext

        if request.CategoryId.IsNone
            || Validation.categoryBelongsTo request.CategoryId.Value userId connectionString
        then
            Success request
        else
            Failure "Category is not valid"

    let private validateCreateAmount (request: CreateUpcomingExpenseRequest) =
        if Validation.amountIsValid request.Amount then
            Success request
        else
            Failure "Amount has to be a positive number"

    let private validateCreateCurrency (request: CreateUpcomingExpenseRequest) =
        if Validation.currencyIsValid request.Currency then
            Success request
        else
            Failure "Currency is not valid"

    let private validateCreateDescription (request: CreateUpcomingExpenseRequest) =
        if Validation.textIsNoneOrLengthIsValid request.Description descriptionMaxLength then
            Success request
        else
            Failure $"Description cannot exceed {descriptionMaxLength} characters"

    let validateCreate =
        validateCreateCategory
        >> bind validateCreateAmount
        >> bind validateCreateCurrency
        >> bind validateCreateDescription

    let createRequestToEntity (request: CreateUpcomingExpenseRequest) (userId: int) =
        { Id = 0
          UserId = userId
          CategoryId = request.CategoryId
          Amount = request.Amount
          Currency = request.Currency
          Description = Utils.noneOrTrimmed request.Description
          Date = request.Date
          Generated = request.Generated
          CreatedDate = request.CreatedDate
          ModifiedDate = request.ModifiedDate }

    type UpdateValidationParams =
        { CurrentUserId: int
          Request: UpdateUpcomingExpenseRequest
          ExistingUpcomingExpense: UpcomingExpense option
          ExistingCategory: Category option }

    let private validateUpdateUpcomingExpense (parameters: UpdateValidationParams) =
        match parameters.ExistingUpcomingExpense with
        | Some upcomingExpense ->
            if upcomingExpense.UserId = parameters.CurrentUserId then
                Success parameters
            else
                Failure "Upcoming expense does not belong to user"
        | None -> Failure "Upcoming expense does not exist"

    let private validateUpdateCategory (parameters: UpdateValidationParams) =
        match parameters.ExistingCategory with
        | Some category ->
            if category.UserId = parameters.CurrentUserId then
                Success parameters
            else
                Failure "Category does not belong to user"
        | None -> Failure "Category does not exist"

    let private validateUpdateAmount (parameters: UpdateValidationParams) =
        if Validation.amountIsValid parameters.Request.Amount then
            Success parameters
        else
            Failure "Amount has to be a positive number"

    let private validateUpdateCurrency (parameters: UpdateValidationParams) =
        if Validation.currencyIsValid parameters.Request.Currency then
            Success parameters
        else
            Failure "Currency is not valid"

    let private validateUpdateDescription (parameters: UpdateValidationParams) =
        if Validation.textIsNoneOrLengthIsValid parameters.Request.Description descriptionMaxLength then
            Success parameters
        else
            Failure $"Description cannot exceed {descriptionMaxLength} characters"

    let validateUpdate =
        validateUpdateUpcomingExpense
        >> bind validateUpdateCategory
        >> bind validateUpdateAmount
        >> bind validateUpdateCurrency
        >> bind validateUpdateDescription

    let updateRequestToEntity (request: UpdateUpcomingExpenseRequest) (userId: int) =
        { Id = request.Id
          UserId = userId
          CategoryId = request.CategoryId
          Amount = request.Amount
          Currency = request.Currency
          Description = Utils.noneOrTrimmed request.Description
          Date = request.Date
          Generated = request.Generated
          CreatedDate = request.CreatedDate
          ModifiedDate = request.ModifiedDate }

    let getFirstDayOfMonth =
        new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0)
