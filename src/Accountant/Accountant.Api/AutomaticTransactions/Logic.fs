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

    let prepareForCreate (request: CreateAutomaticTransactionRequest) (userId: int) =
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

    let private validateUpdateAutomaticTransaction (request: UpdateAutomaticTransactionRequest) =
        let userId = getUserId request.HttpContext
        let connectionString = getConnectionString request.HttpContext

        if Validation.automaticTransactionBelongsTo request.Id userId connectionString then
            Success request
        else
            Failure "Automatic transaction is not valid"

    let private validateUpdateCategory (request: UpdateAutomaticTransactionRequest) =
        let userId = getUserId request.HttpContext
        let connectionString = getConnectionString request.HttpContext

        if
            request.CategoryId.IsNone
            || Validation.categoryBelongsTo request.CategoryId.Value userId connectionString
        then
            Success request
        else
            Failure "Category is not valid"

    let private validateUpdateAmount (request: UpdateAutomaticTransactionRequest) =
        if Validation.amountIsValid request.Amount then
            Success request
        else
            Failure "Amount has to be a positive number"

    let private validateUpdateCurrency (request: UpdateAutomaticTransactionRequest) =
        if Validation.currencyIsValid request.Currency then
            Success request
        else
            Failure "Currency is not valid"

    let private validateUpdateDescription (request: UpdateAutomaticTransactionRequest) =
        if Validation.textIsNoneOrLengthIsValid request.Description descriptionMaxLength then
            Success request
        else
            Failure $"Description cannot exceed {descriptionMaxLength} characters"

    let validateUpdate =
        validateUpdateAutomaticTransaction
        >> bind validateUpdateCategory
        >> bind validateUpdateAmount
        >> bind validateUpdateCurrency
        >> bind validateUpdateDescription

    let prepareForUpdate (request: UpdateAutomaticTransactionRequest) (userId: int) =
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
