namespace Accountant.Api.Accounts

open Accountant.Persistence.Models
open Models
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

    let private validateCreateName (request: CreateAccountRequest) =
        if
            (Validation.textIsNotEmpty request.Name)
            && (Validation.textLengthIsValid request.Name nameMaxLength)
        then
            Success request
        else
            Failure "Name is not valid"

    let private validateCreateCurrency (request: CreateAccountRequest) =
        if Validation.currencyIsValid request.Currency then
            Success request
        else
            Failure "Currency is not valid"

    let validateCreate = validateCreateName >> bind validateCreateCurrency

    let prepareForCreate (request: CreateAccountRequest) (userId: int) =
        { Id = 0
          UserId = userId
          Name = request.Name.Trim()
          IsMain = false
          Currency = request.Currency
          StockPrice = request.StockPrice
          CreatedDate = request.CreatedDate
          ModifiedDate = request.ModifiedDate }

    let private validateUpdateAccount (request: UpdateAccountRequest) =
        let userId = getUserId request.HttpContext
        let connectionString = getConnectionString request.HttpContext

        if Validation.accountBelongsTo request.Id userId connectionString then
            Success request
        else
            Failure "Account is not valid"

    let private validateUpdateName (request: UpdateAccountRequest) =
        if
            (Validation.textIsNotEmpty request.Name)
            && (Validation.textLengthIsValid request.Name nameMaxLength)
        then
            Success request
        else
            Failure "Name is not valid"

    let private validateUpdateCurrency (request: UpdateAccountRequest) =
        if Validation.currencyIsValid request.Currency then
            Success request
        else
            Failure "Currency is not valid"

    let validateUpdate = validateUpdateAccount >> bind validateUpdateName >> bind validateUpdateCurrency

    let prepareForUpdate (request: UpdateAccountRequest) (userId: int) =
        { Id = request.Id
          UserId = userId
          Name = request.Name.Trim()
          IsMain = false
          Currency = request.Currency
          StockPrice = request.StockPrice
          CreatedDate = request.CreatedDate
          ModifiedDate = request.ModifiedDate }
