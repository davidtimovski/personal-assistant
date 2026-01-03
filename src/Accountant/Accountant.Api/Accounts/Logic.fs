namespace Accountant.Api.Accounts

open Accountant.Persistence.Models
open Models

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

    let validateCreate =
        validateCreateName
        >> bind validateCreateCurrency

    let createRequestToEntity (request: CreateAccountRequest) (userId: int) =
        { Id = 0
          UserId = userId
          Name = request.Name.Trim()
          IsMain = false
          Currency = request.Currency
          StockPrice = request.StockPrice
          CreatedDate = request.CreatedDate
          ModifiedDate = request.ModifiedDate }

    type UpdateValidationParams =
        { CurrentUserId: int
          Request: UpdateAccountRequest
          ExistingAccount: Account option }

    let private validateUpdateAccount (parameters: UpdateValidationParams) =
        match parameters.ExistingAccount with
        | Some account ->
            if account.UserId = parameters.CurrentUserId then
                Success parameters
            else
                Failure "Account does not belong to user"
        | None -> Failure "Account does not exist"

    let private validateUpdateName (parameters: UpdateValidationParams) =
        if (Validation.textIsNotEmpty parameters.Request.Name)
            && (Validation.textLengthIsValid parameters.Request.Name nameMaxLength)
        then
            Success parameters
        else
            Failure "Name is not valid"

    let private validateUpdateCurrency (parameters: UpdateValidationParams) =
        if Validation.currencyIsValid parameters.Request.Currency then
            Success parameters
        else
            Failure "Currency is not valid"

    let validateUpdate =
        validateUpdateAccount
        >> bind validateUpdateName
        >> bind validateUpdateCurrency

    let updateRequestToEntity (request: UpdateAccountRequest) (userId: int) =
        { Id = request.Id
          UserId = userId
          Name = request.Name.Trim()
          IsMain = false
          Currency = request.Currency
          StockPrice = request.StockPrice
          CreatedDate = request.CreatedDate
          ModifiedDate = request.ModifiedDate }
