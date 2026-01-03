namespace Accountant.Api.Debts

open Accountant.Persistence.Models
open Models
open CommonHandlers

module Logic =
    [<Literal>]
    let private descriptionMaxLength = 5000

    let mapAll (debts: seq<Debt>) : seq<DebtDto> =
        debts
        |> Seq.map (fun x ->
            { Id = x.Id
              Person = x.Person
              Amount = x.Amount
              Currency = x.Currency
              Description = x.Description
              UserIsDebtor = x.UserIsDebtor
              CreatedDate = x.CreatedDate
              ModifiedDate = x.ModifiedDate })

    let private validateCreatePerson (request: CreateDebtRequest) =
        if Validation.textIsNotEmpty request.Person then
            Success request
        else
            Failure "Person is not valid"

    let private validateCreateAmount (request: CreateDebtRequest) =
        if Validation.amountIsValid request.Amount then
            Success request
        else
            Failure "Amount has to be a positive number"

    let private validateCreateCurrency (request: CreateDebtRequest) =
        if Validation.currencyIsValid request.Currency then
            Success request
        else
            Failure "Currency is not valid"

    let private validateCreateDescription (request: CreateDebtRequest) =
        if Validation.textIsNoneOrLengthIsValid request.Description descriptionMaxLength then
            Success request
        else
            Failure $"Description cannot exceed {descriptionMaxLength} characters"

    let validateCreate =
        validateCreatePerson
        >> bind validateCreateAmount
        >> bind validateCreateCurrency
        >> bind validateCreateDescription

    let createRequestToEntity (request: CreateDebtRequest) (userId: int) =
        { Id = 0
          UserId = userId
          Person = request.Person.Trim()
          Amount = request.Amount
          Currency = request.Currency
          Description = Utils.noneOrTrimmed request.Description
          UserIsDebtor = request.UserIsDebtor
          CreatedDate = request.CreatedDate
          ModifiedDate = request.ModifiedDate }

    let createMergedRequestToEntity (request: CreateDebtRequest) (userId: int) =
        { Id = 0
          UserId = userId
          Person = request.Person.Trim()
          Amount = request.Amount
          Currency = request.Currency
          Description = Utils.noneOrTrimmed request.Description
          UserIsDebtor = request.UserIsDebtor
          CreatedDate = request.CreatedDate
          ModifiedDate = request.ModifiedDate }

    type UpdateValidationParams =
        { CurrentUserId: int
          Request: UpdateDebtRequest
          ExistingDebt: Debt option }

    let private validateUpdateDebt (parameters: UpdateValidationParams) =
        match parameters.ExistingDebt with
        | Some debt ->
            if debt.UserId = parameters.CurrentUserId then
                Success parameters
            else
                Failure "Debt does not belong to user"
        | None -> Failure "Debt does not exist"

    let private validateUpdatePerson (parameters: UpdateValidationParams) =
        if Validation.textIsNotEmpty parameters.Request.Person then
            Success parameters
        else
            Failure "Person is not valid"

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
        validateUpdateDebt
        >> bind validateUpdatePerson
        >> bind validateUpdateAmount
        >> bind validateUpdateCurrency
        >> bind validateUpdateDescription

    let updateRequestToEntity (request: UpdateDebtRequest) (userId: int) =
        { Id = request.Id
          UserId = userId
          Person = request.Person.Trim()
          Amount = request.Amount
          Currency = request.Currency
          Description = Utils.noneOrTrimmed request.Description
          UserIsDebtor = request.UserIsDebtor
          CreatedDate = request.CreatedDate
          ModifiedDate = request.ModifiedDate }
