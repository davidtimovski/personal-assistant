module HandlerBase

open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Localization

let localize (ctx: HttpContext) text =
    let enTranslations = Map ["Encrypted", "[ Encrypted ]"; "Uncategorized", "Uncategorized"]
    let mkTranslations = Map ["Encrypted", "[ Шифриран ]"; "Uncategorized", "Некатегоризирано"]
    let lookup = Map ["en-US", enTranslations; "mk-MK", mkTranslations]

    let rqf = ctx.Request.HttpContext.Features.Get<IRequestCultureFeature>();

    lookup[rqf.RequestCulture.Culture.Name][text]
