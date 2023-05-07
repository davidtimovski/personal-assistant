namespace Api.Common.Fs

open Sentry

module Metrics =

    let startTransaction (name: string) (operation: string) =
        SentrySdk.ConfigureScope(fun scope -> scope.TransactionName <- name)
        SentrySdk.StartTransaction(name, operation)

    let startTransactionWithUser (name: string) (operation: string) (userId: int) =
        let tr = startTransaction name operation
        tr.User <- new User(Id = userId.ToString())
        tr
