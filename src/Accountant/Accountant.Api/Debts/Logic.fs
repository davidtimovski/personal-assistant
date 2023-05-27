namespace Accountant.Api.Debts

open Accountant.Persistence.Fs.Models
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
              UserIsDebtor = x.UserIsDebtor
              Description = x.Description
              CreatedDate = x.CreatedDate
              ModifiedDate = x.ModifiedDate })

    let private validateCreatePerson (dto: CreateDebt) =
        if Validation.textIsNotEmpty dto.Person then
            Success dto
        else
            Failure "Person is not valid"

    let private validateCreateAmount (dto: CreateDebt) =
        if Validation.amountIsValid dto.Amount then
            Success dto
        else
            Failure "Amount has to be a positive number"

    let private validateCreateCurrency (dto: CreateDebt) =
        if Validation.currencyIsValid dto.Currency then
            Success dto
        else
            Failure "Currency is not valid"

    let private validateCreateDescription (dto: CreateDebt) =
        if Validation.textIsNoneOrLengthIsValid dto.Description descriptionMaxLength then
            Success dto
        else
            Failure $"Description cannot exceed {descriptionMaxLength} characters"

    let validateCreate =
        validateCreatePerson
        >> bind validateCreateAmount
        >> bind validateCreateCurrency
        >> bind validateCreateDescription

    let prepareForCreate (model: CreateDebt) (userId: int) =
        { Id = 0
          UserId = userId
          Person = model.Person.Trim()
          Amount = model.Amount
          Currency = model.Currency
          UserIsDebtor = model.UserIsDebtor
          Description = Utils.noneOrTrimmed model.Description
          CreatedDate = model.CreatedDate
          ModifiedDate = model.ModifiedDate }

    let prepareForCreateMerged (model: CreateDebt) (userId: int) =
        { Id = 0
          UserId = userId
          Person = model.Person.Trim()
          Amount = model.Amount
          Currency = model.Currency
          UserIsDebtor = model.UserIsDebtor
          Description = Utils.noneOrTrimmed model.Description
          CreatedDate = model.CreatedDate
          ModifiedDate = model.ModifiedDate }

    let private validateUpdateDebt (dto: UpdateDebt) =
        let userId = getUserId dto.HttpContext
        let connectionString = getConnectionString dto.HttpContext

        if Validation.debtBelongsTo dto.Id userId connectionString then
            Success dto
        else
            Failure "Debt is not valid"

    let private validateUpdatePerson (dto: UpdateDebt) =
        if Validation.textIsNotEmpty dto.Person then
            Success dto
        else
            Failure "Person is not valid"

    let private validateUpdateAmount (dto: UpdateDebt) =
        if Validation.amountIsValid dto.Amount then
            Success dto
        else
            Failure "Amount has to be a positive number"

    let private validateUpdateCurrency (dto: UpdateDebt) =
        if Validation.currencyIsValid dto.Currency then
            Success dto
        else
            Failure "Currency is not valid"

    let private validateUpdateDescription (dto: UpdateDebt) =
        if Validation.textIsNoneOrLengthIsValid dto.Description descriptionMaxLength then
            Success dto
        else
            Failure $"Description cannot exceed {descriptionMaxLength} characters"

    let validateUpdate =
        validateUpdateDebt
        >> bind validateUpdatePerson
        >> bind validateUpdateAmount
        >> bind validateUpdateCurrency
        >> bind validateUpdateDescription

    let prepareForUpdate (model: UpdateDebt) (userId: int) =
        { Id = model.Id
          UserId = userId
          Person = model.Person.Trim()
          Amount = model.Amount
          Currency = model.Currency
          UserIsDebtor = model.UserIsDebtor
          Description = Utils.noneOrTrimmed model.Description
          CreatedDate = model.CreatedDate
          ModifiedDate = model.ModifiedDate }
