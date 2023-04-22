namespace Accountant.Api.Accounts

open Accountant.Persistence.Fs.Models
open Models
open Accountant.Api.HandlerBase
open CommonHandlers

module Logic =
    [<Literal>]
    let private nameMaxLength = 30

    let mapAll (accounts: seq<Account>) : seq<AccountDto> =
        accounts
        |> Seq.map (fun x ->
            { Id = x.Id
              Name = x.Name
              IsMain = x.IsMain
              Currency = x.Currency
              StockPrice = x.StockPrice
              CreatedDate = x.CreatedDate
              ModifiedDate = x.ModifiedDate })

    let private validateCreateName (dto: CreateAccount) =
        if
            (Validation.textIsNotEmpty dto.Name)
            && (Validation.textLengthIsValid dto.Name nameMaxLength)
        then
            Success dto
        else
            Failure "Name is not valid"

    let private validateCreateCurrency (dto: CreateAccount) =
        if Validation.currencyIsValid dto.Currency then
            Success dto
        else
            Failure "Currency is not valid"

    let validateCreate = validateCreateName >> bind validateCreateCurrency

    let prepareForCreate (model: CreateAccount) (userId: int) =
        { Id = 0
          UserId = userId
          Name = model.Name.Trim()
          IsMain = model.IsMain
          Currency = model.Currency
          StockPrice = model.StockPrice
          CreatedDate = model.CreatedDate
          ModifiedDate = model.ModifiedDate }

    let private validateUpdateAccount (dto: UpdateAccount) =
        let userId = getUserId dto.HttpContext
        let connection = getDbConnection dto.HttpContext

        if Validation.accountBelongsTo dto.Id userId connection then
            Success dto
        else
            Failure "Account is not valid"

    let private validateUpdateName (dto: UpdateAccount) =
        if
            (Validation.textIsNotEmpty dto.Name)
            && (Validation.textLengthIsValid dto.Name nameMaxLength)
        then
            Success dto
        else
            Failure "Name is not valid"

    let private validateUpdateCurrency (dto: UpdateAccount) =
        if Validation.currencyIsValid dto.Currency then
            Success dto
        else
            Failure "Currency is not valid"

    let validateUpdate = validateUpdateAccount >> bind validateUpdateName >> bind validateUpdateCurrency

    let prepareForUpdate (model: UpdateAccount) (userId: int) =
        { Id = model.Id
          UserId = userId
          Name = model.Name.Trim()
          IsMain = model.IsMain
          Currency = model.Currency
          StockPrice = model.StockPrice
          CreatedDate = model.CreatedDate
          ModifiedDate = model.ModifiedDate }
