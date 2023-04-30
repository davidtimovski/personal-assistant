namespace Accountant.Api

open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Localization
open Accountant.Persistence.Fs.ConnectionUtils
open Sentry
open CommonHandlers

module HandlerBase =

    let localize (ctx: HttpContext) text =
        let enTranslations =
            Map [ "Encrypted", "[ Encrypted ]"; "Uncategorized", "Uncategorized" ]

        let mkTranslations =
            Map [ "Encrypted", "[ Шифриран ]"; "Uncategorized", "Некатегоризирано" ]

        let lookup = Map [ "en-US", enTranslations; "mk-MK", mkTranslations ]

        let rqf = ctx.Request.HttpContext.Features.Get<IRequestCultureFeature>()

        lookup[rqf.RequestCulture.Culture.Name][text]

    let getDbConnection (ctx: HttpContext) =
        ConnectionString(CommonHandlers.getConnectionString ctx)

    let startTransactionWithUser (name: string) (operation: string) (ctx: HttpContext) =
        let userId = getUserId ctx
        let tr = SentrySdk.StartTransaction(name, operation)
        tr.User <- new User(Id = userId.ToString())
        tr
