namespace Accountant.Api.AutomaticTransactions

open Accountant.Persistence.Fs
open Accountant.Api.HandlerBase
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

    let private validateCreateCategory (dto: CreateAutomaticTransaction) =
        let userId = getUserId dto.HttpContext
        let connection = getDbConnection dto.HttpContext

        if
            dto.CategoryId.IsNone
            || Validation.categoryBelongsTo dto.CategoryId.Value userId connection
        then
            Success dto
        else
            Failure "Category is not valid"

    let private validateCreateAmount (dto: CreateAutomaticTransaction) =
        if Validation.amountIsValid dto.Amount then
            Success dto
        else
            Failure "Amount has to be a positive number"

    let private validateCreateCurrency (dto: CreateAutomaticTransaction) =
        if Validation.currencyIsValid dto.Currency then
            Success dto
        else
            Failure "Currency is not valid"

    let private validateCreateDescription (dto: CreateAutomaticTransaction) =
        if Validation.textIsNoneOrLengthIsValid dto.Description descriptionMaxLength then
            Success dto
        else
            Failure $"Description cannot exceed {descriptionMaxLength} characters"

    let validateCreate =
        validateCreateCategory
        >> bind validateCreateAmount
        >> bind validateCreateCurrency
        >> bind validateCreateDescription

    let prepareForCreate (model: CreateAutomaticTransaction) (userId: int) =
        { Id = 0
          UserId = userId
          IsDeposit = model.IsDeposit
          CategoryId = model.CategoryId
          Amount = model.Amount
          Currency = model.Currency
          Description = Utils.noneOrTrimmed model.Description
          DayInMonth = model.DayInMonth
          CreatedDate = model.CreatedDate
          ModifiedDate = model.ModifiedDate }

    let private validateUpdateAutomaticTransaction (dto: UpdateAutomaticTransaction) =
        let userId = getUserId dto.HttpContext
        let connection = getDbConnection dto.HttpContext

        if Validation.automaticTransactionBelongsTo dto.Id userId connection then
            Success dto
        else
            Failure "Automatic transaction is not valid"

    let private validateUpdateCategory (dto: UpdateAutomaticTransaction) =
        let userId = getUserId dto.HttpContext
        let connection = getDbConnection dto.HttpContext

        if
            dto.CategoryId.IsNone
            || Validation.categoryBelongsTo dto.CategoryId.Value userId connection
        then
            Success dto
        else
            Failure "Category is not valid"

    let private validateUpdateAmount (dto: UpdateAutomaticTransaction) =
        if Validation.amountIsValid dto.Amount then
            Success dto
        else
            Failure "Amount has to be a positive number"

    let private validateUpdateCurrency (dto: UpdateAutomaticTransaction) =
        if Validation.currencyIsValid dto.Currency then
            Success dto
        else
            Failure "Currency is not valid"

    let private validateUpdateDescription (dto: UpdateAutomaticTransaction) =
        if Validation.textIsNoneOrLengthIsValid dto.Description descriptionMaxLength then
            Success dto
        else
            Failure $"Description cannot exceed {descriptionMaxLength} characters"

    let validateUpdate =
        validateUpdateAutomaticTransaction
        >> bind validateUpdateCategory
        >> bind validateUpdateAmount
        >> bind validateUpdateCurrency
        >> bind validateUpdateDescription

    let prepareForUpdate (model: UpdateAutomaticTransaction) (userId: int) =
        { Id = model.Id
          UserId = userId
          IsDeposit = model.IsDeposit
          CategoryId = model.CategoryId
          Amount = model.Amount
          Currency = model.Currency
          Description = Utils.noneOrTrimmed model.Description
          DayInMonth = model.DayInMonth
          CreatedDate = model.CreatedDate
          ModifiedDate = model.ModifiedDate }
