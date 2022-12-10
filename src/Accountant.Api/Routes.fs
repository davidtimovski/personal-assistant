module Routes

open Giraffe
open Microsoft.AspNetCore.Authentication.JwtBearer
open Microsoft.AspNetCore.Http

let authorize: HttpHandler =
    requiresAuthentication (challenge JwtBearerDefaults.AuthenticationScheme)

let webApp: HttpHandler =
    subRoute
        "/api"
        (choose
            [ POST
              >=> authorize
              >=> choose
                      [ route "/sync/changes" >=> SyncHandlers.getChanges
                        route "/sync/create-entities" >=> SyncHandlers.createEntities

                        route "/accounts" >=> AccountHandlers.create
                        route "/categories" >=> CategoryHandlers.create
                        route "/transactions" >=> TransactionHandlers.create
                        route "/transactions/export" >=> TransactionHandlers.export
                        route "/upcoming-expenses" >=> UpcomingExpenseHandlers.create
                        route "/debts" >=> DebtHandlers.create
                        route "/debts/merged" >=> DebtHandlers.createMerged
                        route "/automatic-transactions" >=> AutomaticTransactionHandlers.create ]
              PUT
              >=> authorize
              >=> choose
                      [ route "/accounts" >=> AccountHandlers.update
                        route "/categories" >=> CategoryHandlers.update
                        route "/transactions" >=> TransactionHandlers.update
                        route "/upcoming-expenses" >=> UpcomingExpenseHandlers.update
                        route "/debts" >=> DebtHandlers.update
                        route "/automatic-transactions" >=> AutomaticTransactionHandlers.update ]
              DELETE
              >=> authorize
              >=> choose
                      [ routef "/accounts/%i" AccountHandlers.delete
                        routef "/categories/%i" CategoryHandlers.delete
                        routef "/transactions/%i" TransactionHandlers.delete
                        routef "/transactions/exported-file/%O" TransactionHandlers.deleteExportedFile
                        routef "/upcoming-expenses/%i" UpcomingExpenseHandlers.delete
                        routef "/debts/%i" DebtHandlers.delete
                        routef "/automatic-transactions/%i" AutomaticTransactionHandlers.delete ]
              setStatusCode StatusCodes.Status404NotFound >=> text "Not Found" ])
