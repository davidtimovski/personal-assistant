namespace Accountant.Api

open Giraffe
open Microsoft.AspNetCore.Http
open Accountant.Api
open CommonHandlers

module Routes =

    let webApp: HttpHandler =
        subRoute
            "/api"
            (choose
                [ POST
                  >=> authorize
                  >=> choose
                          [ route "/sync/changes" >=> Sync.Handlers.getChanges
                            route "/sync/create-entities" >=> Sync.Handlers.createEntities

                            route "/accounts" >=> Accounts.Handlers.create
                            route "/categories" >=> Categories.Handlers.create
                            route "/transactions" >=> Transactions.Handlers.create
                            route "/transactions/export" >=> Transactions.Handlers.export
                            route "/upcoming-expenses" >=> UpcomingExpenses.Handlers.create
                            route "/debts" >=> Debts.Handlers.create
                            route "/debts/merged" >=> Debts.Handlers.createMerged
                            route "/automatic-transactions" >=> AutomaticTransactions.Handlers.create ]
                  PUT
                  >=> authorize
                  >=> choose
                          [ route "/accounts" >=> Accounts.Handlers.update
                            route "/categories" >=> Categories.Handlers.update
                            route "/transactions" >=> Transactions.Handlers.update
                            route "/upcoming-expenses" >=> UpcomingExpenses.Handlers.update
                            route "/debts" >=> Debts.Handlers.update
                            route "/automatic-transactions" >=> AutomaticTransactions.Handlers.update ]
                  DELETE
                  >=> authorize
                  >=> choose
                          [ routef "/accounts/%i" Accounts.Handlers.delete
                            routef "/categories/%i" Categories.Handlers.delete
                            routef "/transactions/%i" Transactions.Handlers.delete
                            routef "/transactions/exported-file/%O" Transactions.Handlers.deleteExportedFile
                            routef "/upcoming-expenses/%i" UpcomingExpenses.Handlers.delete
                            routef "/debts/%i" Debts.Handlers.delete
                            routef "/automatic-transactions/%i" AutomaticTransactions.Handlers.delete ]
                  setStatusCode StatusCodes.Status404NotFound >=> text "Not Found" ])
