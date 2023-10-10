namespace Accountant.Api.Transactions

open Accountant.Persistence.Fs
open Models
open CommonHandlers

module Logic =
    [<Literal>]
    let private descriptionMaxLength = 500

    let private validateAccounts (fromAccountId: int Option) (toAccountId: int Option) (userId: int) connection =
        (fromAccountId.IsSome || toAccountId.IsSome)
        && (fromAccountId <> toAccountId)
        && fromAccountId.IsNone
        || (Validation.accountBelongsTo fromAccountId.Value userId connection)
           && toAccountId.IsNone
        || (Validation.accountBelongsTo toAccountId.Value userId connection)

    let mapAll (categories: seq<Transaction>) : seq<TransactionDto> =
        categories
        |> Seq.map (fun x ->
            { Id = x.Id
              FromAccountId = x.FromAccountId
              ToAccountId = x.ToAccountId
              CategoryId = x.CategoryId
              Amount = x.Amount
              FromStocks = x.FromStocks
              ToStocks = x.ToStocks
              Currency = x.Currency
              Description = x.Description
              Date = x.Date
              IsEncrypted = x.IsEncrypted
              EncryptedDescription = x.EncryptedDescription
              Salt = x.Salt
              Nonce = x.Nonce
              Generated = x.Generated
              CreatedDate = x.CreatedDate
              ModifiedDate = x.ModifiedDate })

    let private validateCreateAccounts (request: CreateTransactionRequest) =
        let userId = getUserId request.HttpContext
        let connectionString = getConnectionString request.HttpContext

        if validateAccounts request.FromAccountId request.ToAccountId userId connectionString then
            Success request
        else
            Failure "Accounts are not valid"

    let private validateCreateCategory (request: CreateTransactionRequest) =
        let userId = getUserId request.HttpContext
        let connectionString = getConnectionString request.HttpContext

        if
            request.CategoryId.IsNone
            || Validation.categoryBelongsTo request.CategoryId.Value userId connectionString
        then
            Success request
        else
            Failure "Category is not valid"

    let private validateCreateAmount (request: CreateTransactionRequest) =
        if Validation.amountIsValid request.Amount then
            Success request
        else
            Failure "Amount has to be a positive number"

    let private validateCreateCurrency (request: CreateTransactionRequest) =
        if Validation.currencyIsValid request.Currency then
            Success request
        else
            Failure "Currency is not valid"

    let private validateCreateDescription (request: CreateTransactionRequest) =
        if Validation.textIsNoneOrLengthIsValid request.Description descriptionMaxLength then
            Success request
        else
            Failure $"Description cannot exceed {descriptionMaxLength} characters"

    let validateCreate =
        validateCreateAccounts
        >> bind validateCreateCategory
        >> bind validateCreateAmount
        >> bind validateCreateCurrency
        >> bind validateCreateDescription

    let prepareForCreate (request: CreateTransactionRequest) : Transaction =
        { Id = 0
          FromAccountId = request.FromAccountId
          ToAccountId = request.ToAccountId
          CategoryId = request.CategoryId
          Amount = request.Amount
          FromStocks = request.FromStocks
          ToStocks = request.ToStocks
          Currency = request.Currency
          Description = Utils.noneOrTrimmed request.Description
          Date = request.Date
          IsEncrypted = request.IsEncrypted
          EncryptedDescription = request.EncryptedDescription
          Salt = request.Salt
          Nonce = request.Nonce
          Generated = false
          CreatedDate = request.CreatedDate
          ModifiedDate = request.ModifiedDate }

    let private validateUpdateTransaction (request: UpdateTransactionRequest) =
        let userId = getUserId request.HttpContext
        let connectionString = getConnectionString request.HttpContext

        if Validation.transactionBelongsTo request.Id userId connectionString then
            Success request
        else
            Failure "Transaction is not valid"

    let private validateUpdateAccounts (request: UpdateTransactionRequest) =
        let userId = getUserId request.HttpContext
        let connectionString = getConnectionString request.HttpContext

        if validateAccounts request.FromAccountId request.ToAccountId userId connectionString then
            Success request
        else
            Failure "Accounts are not valid"

    let private validateUpdateCategory (request: UpdateTransactionRequest) =
        let userId = getUserId request.HttpContext
        let connectionString = getConnectionString request.HttpContext

        if
            request.CategoryId.IsNone
            || Validation.categoryBelongsTo request.CategoryId.Value userId connectionString
        then
            Success request
        else
            Failure "Category is not valid"

    let private validateUpdateAmount (request: UpdateTransactionRequest) =
        if Validation.amountIsValid request.Amount then
            Success request
        else
            Failure "Amount has to be a positive number"

    let private validateUpdateCurrency (request: UpdateTransactionRequest) =
        if Validation.currencyIsValid request.Currency then
            Success request
        else
            Failure "Currency is not valid"

    let private validateUpdateDescription (request: UpdateTransactionRequest) =
        if Validation.textIsNoneOrLengthIsValid request.Description descriptionMaxLength then
            Success request
        else
            Failure $"Description cannot exceed {descriptionMaxLength} characters"

    let validateUpdate =
        validateUpdateTransaction
        >> bind validateUpdateAccounts
        >> bind validateUpdateCategory
        >> bind validateUpdateAmount
        >> bind validateUpdateCurrency
        >> bind validateUpdateDescription

    let prepareForUpdate (request: UpdateTransactionRequest) : Transaction =
        { Id = request.Id
          FromAccountId = request.FromAccountId
          ToAccountId = request.ToAccountId
          CategoryId = request.CategoryId
          Amount = request.Amount
          FromStocks = request.FromStocks
          ToStocks = request.ToStocks
          Currency = request.Currency
          Description = Utils.noneOrTrimmed request.Description
          Date = request.Date
          IsEncrypted = request.IsEncrypted
          EncryptedDescription = request.EncryptedDescription
          Salt = request.Salt
          Nonce = request.Nonce
          Generated = false
          CreatedDate = request.CreatedDate
          ModifiedDate = request.ModifiedDate }
