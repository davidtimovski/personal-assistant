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

    let private validateCreateAccounts (dto: CreateTransaction) =
        let userId = getUserId dto.HttpContext
        let connectionString = getConnectionString dto.HttpContext

        if validateAccounts dto.FromAccountId dto.ToAccountId userId connectionString then
            Success dto
        else
            Failure "Accounts are not valid"

    let private validateCreateCategory (dto: CreateTransaction) =
        let userId = getUserId dto.HttpContext
        let connectionString = getConnectionString dto.HttpContext

        if
            dto.CategoryId.IsNone
            || Validation.categoryBelongsTo dto.CategoryId.Value userId connectionString
        then
            Success dto
        else
            Failure "Category is not valid"

    let private validateCreateAmount (dto: CreateTransaction) =
        if Validation.amountIsValid dto.Amount then
            Success dto
        else
            Failure "Amount has to be a positive number"

    let private validateCreateCurrency (dto: CreateTransaction) =
        if Validation.currencyIsValid dto.Currency then
            Success dto
        else
            Failure "Currency is not valid"

    let private validateCreateDescription (dto: CreateTransaction) =
        if Validation.textIsNoneOrLengthIsValid dto.Description descriptionMaxLength then
            Success dto
        else
            Failure $"Description cannot exceed {descriptionMaxLength} characters"

    let validateCreate =
        validateCreateAccounts
        >> bind validateCreateCategory
        >> bind validateCreateAmount
        >> bind validateCreateCurrency
        >> bind validateCreateDescription

    let prepareForCreate (model: CreateTransaction) : Transaction =
        { Id = 0
          FromAccountId = model.FromAccountId
          ToAccountId = model.ToAccountId
          CategoryId = model.CategoryId
          Amount = model.Amount
          FromStocks = model.FromStocks
          ToStocks = model.ToStocks
          Currency = model.Currency
          Description = Utils.noneOrTrimmed model.Description
          Date = model.Date
          IsEncrypted = model.IsEncrypted
          EncryptedDescription = model.EncryptedDescription
          Salt = model.Salt
          Nonce = model.Nonce
          Generated = false
          CreatedDate = model.CreatedDate
          ModifiedDate = model.ModifiedDate }

    let private validateUpdateTransaction (dto: UpdateTransaction) =
        let userId = getUserId dto.HttpContext
        let connectionString = getConnectionString dto.HttpContext

        if Validation.transactionBelongsTo dto.Id userId connectionString then
            Success dto
        else
            Failure "Transaction is not valid"

    let private validateUpdateAccounts (dto: UpdateTransaction) =
        let userId = getUserId dto.HttpContext
        let connectionString = getConnectionString dto.HttpContext

        if validateAccounts dto.FromAccountId dto.ToAccountId userId connectionString then
            Success dto
        else
            Failure "Accounts are not valid"

    let private validateUpdateCategory (dto: UpdateTransaction) =
        let userId = getUserId dto.HttpContext
        let connectionString = getConnectionString dto.HttpContext

        if
            dto.CategoryId.IsNone
            || Validation.categoryBelongsTo dto.CategoryId.Value userId connectionString
        then
            Success dto
        else
            Failure "Category is not valid"

    let private validateUpdateAmount (dto: UpdateTransaction) =
        if Validation.amountIsValid dto.Amount then
            Success dto
        else
            Failure "Amount has to be a positive number"

    let private validateUpdateCurrency (dto: UpdateTransaction) =
        if Validation.currencyIsValid dto.Currency then
            Success dto
        else
            Failure "Currency is not valid"

    let private validateUpdateDescription (dto: UpdateTransaction) =
        if Validation.textIsNoneOrLengthIsValid dto.Description descriptionMaxLength then
            Success dto
        else
            Failure $"Description cannot exceed {descriptionMaxLength} characters"

    let validateUpdate =
        validateUpdateTransaction
        >> bind validateUpdateAccounts
        >> bind validateUpdateCategory
        >> bind validateUpdateAmount
        >> bind validateUpdateCurrency
        >> bind validateUpdateDescription

    let prepareForUpdate (model: UpdateTransaction) : Transaction =
        { Id = model.Id
          FromAccountId = model.FromAccountId
          ToAccountId = model.ToAccountId
          CategoryId = model.CategoryId
          Amount = model.Amount
          FromStocks = model.FromStocks
          ToStocks = model.ToStocks
          Currency = model.Currency
          Description = Utils.noneOrTrimmed model.Description
          Date = model.Date
          IsEncrypted = model.IsEncrypted
          EncryptedDescription = model.EncryptedDescription
          Salt = model.Salt
          Nonce = model.Nonce
          Generated = false
          CreatedDate = model.CreatedDate
          ModifiedDate = model.ModifiedDate }
