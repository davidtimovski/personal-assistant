namespace Api.Common.Fs

open Sentry

module Metrics =

    let startTransactionWithUser (name: string) (operation: string) (userId: int) =
        SentrySdk.ConfigureScope(fun scope -> scope.TransactionName <- name)
        let tr = SentrySdk.StartTransaction(name, operation)
        tr.User <- new SentryUser(Id = userId.ToString())
        tr
