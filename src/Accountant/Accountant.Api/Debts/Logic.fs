namespace Accountant.Api.Debts

open Accountant.Persistence.Models
open Models
open CommonHandlers

module Logic =
    [<Literal>]
    let private descriptionMaxLength = 2000

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

    let prepareForCreate (request: CreateDebtRequest) (userId: int) =
        { Id = 0
          UserId = userId
          Person = request.Person.Trim()
          Amount = request.Amount
          Currency = request.Currency
          Description = Utils.noneOrTrimmed request.Description
          UserIsDebtor = request.UserIsDebtor
          CreatedDate = request.CreatedDate
          ModifiedDate = request.ModifiedDate }

    let prepareForCreateMerged (request: CreateDebtRequest) (userId: int) =
        { Id = 0
          UserId = userId
          Person = request.Person.Trim()
          Amount = request.Amount
          Currency = request.Currency
          Description = Utils.noneOrTrimmed request.Description
          UserIsDebtor = request.UserIsDebtor
          CreatedDate = request.CreatedDate
          ModifiedDate = request.ModifiedDate }

    let private validateUpdateDebt (request: UpdateDebtRequest) =
        let userId = getUserId request.HttpContext
        let connectionString = getConnectionString request.HttpContext

        if Validation.debtBelongsTo request.Id userId connectionString then
            Success request
        else
            Failure "Debt is not valid"

    let private validateUpdatePerson (request: UpdateDebtRequest) =
        if Validation.textIsNotEmpty request.Person then
            Success request
        else
            Failure "Person is not valid"

    let private validateUpdateAmount (request: UpdateDebtRequest) =
        if Validation.amountIsValid request.Amount then
            Success request
        else
            Failure "Amount has to be a positive number"

    let private validateUpdateCurrency (request: UpdateDebtRequest) =
        if Validation.currencyIsValid request.Currency then
            Success request
        else
            Failure "Currency is not valid"

    let private validateUpdateDescription (request: UpdateDebtRequest) =
        if Validation.textIsNoneOrLengthIsValid request.Description descriptionMaxLength then
            Success request
        else
            Failure $"Description cannot exceed {descriptionMaxLength} characters"

    let validateUpdate =
        validateUpdateDebt
        >> bind validateUpdatePerson
        >> bind validateUpdateAmount
        >> bind validateUpdateCurrency
        >> bind validateUpdateDescription

    let prepareForUpdate (request: UpdateDebtRequest) (userId: int) =
        { Id = request.Id
          UserId = userId
          Person = request.Person.Trim()
          Amount = request.Amount
          Currency = request.Currency
          Description = Utils.noneOrTrimmed request.Description
          UserIsDebtor = request.UserIsDebtor
          CreatedDate = request.CreatedDate
          ModifiedDate = request.ModifiedDate }
