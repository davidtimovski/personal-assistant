namespace Accountant.Api.AutomaticTransactions

open Accountant.Persistence
open CommonHandlers
open Models

module Logic =
    [<Literal>]
    let private descriptionMaxLength = 250

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

    let private validateCreateCategory (request: CreateAutomaticTransactionRequest) =
        let userId = getUserId request.HttpContext
        let connectionString = getConnectionString request.HttpContext

        if
            request.CategoryId.IsNone
            || Validation.categoryBelongsTo request.CategoryId.Value userId connectionString
        then
            Success request
        else
            Failure "Category is not valid"

    let private validateCreateAmount (request: CreateAutomaticTransactionRequest) =
        if Validation.amountIsValid request.Amount then
            Success request
        else
            Failure "Amount has to be a positive number"

    let private validateCreateCurrency (request: CreateAutomaticTransactionRequest) =
        if Validation.currencyIsValid request.Currency then
            Success request
        else
            Failure "Currency is not valid"

    let private validateCreateDescription (request: CreateAutomaticTransactionRequest) =
        if Validation.textIsNoneOrLengthIsValid request.Description descriptionMaxLength then
            Success request
        else
            Failure $"Description cannot exceed {descriptionMaxLength} characters"

    let validateCreate =
        validateCreateCategory
        >> bind validateCreateAmount
        >> bind validateCreateCurrency
        >> bind validateCreateDescription

    let createRequestToEntity (request: CreateAutomaticTransactionRequest) (userId: int) =
        { Id = 0
          UserId = userId
          IsDeposit = request.IsDeposit
          CategoryId = request.CategoryId
          Amount = request.Amount
          Currency = request.Currency
          Description = Utils.noneOrTrimmed request.Description
          DayInMonth = request.DayInMonth
          CreatedDate = request.CreatedDate
          ModifiedDate = request.ModifiedDate }

    type UpdateValidationParams =
        { CurrentUserId: int
          Request: UpdateAutomaticTransactionRequest
          ExistingAutomaticTransaction: AutomaticTransaction option
          ExistingCategory: Category option }

    let private validateUpdateAutomaticTransaction (parameters: UpdateValidationParams) =
        match parameters.ExistingAutomaticTransaction with
        | Some automaticTransaction ->
            if automaticTransaction.UserId = parameters.CurrentUserId then
                Success parameters
            else
                Failure "Automatic transaction does not belong to user"
        | None -> Failure "Automatic transaction does not exist"

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
        validateUpdateAutomaticTransaction
        >> bind validateUpdateCategory
        >> bind validateUpdateAmount
        >> bind validateUpdateCurrency
        >> bind validateUpdateDescription

    let updateRequestToEntity (request: UpdateAutomaticTransactionRequest) (userId: int) =
        { Id = request.Id
          UserId = userId
          IsDeposit = request.IsDeposit
          CategoryId = request.CategoryId
          Amount = request.Amount
          Currency = request.Currency
          Description = Utils.noneOrTrimmed request.Description
          DayInMonth = request.DayInMonth
          CreatedDate = request.CreatedDate
          ModifiedDate = request.ModifiedDate }
