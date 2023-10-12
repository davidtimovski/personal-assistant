module ForecastHandlers

open Giraffe
open Microsoft.AspNetCore.Http
open Weatherman.Application.Contracts.Forecasts
open Weatherman.Application.Contracts.Forecasts.Models
open Api.Common.Fs
open Models
open CommonHandlers

let get: HttpHandler =
    successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
        let userId = getUserId ctx

        let tr =
            Metrics.startTransactionWithUser $"{ctx.Request.Method} /api/forecasts" "ForecastHandlers.get" userId

        let result = ctx.TryBindQueryString<GetForecastDto>()

        (match result with
         | Error _ ->
            tr.Finish()
            RequestErrors.BAD_REQUEST "Bad request" next ctx
         | Ok dto ->
             let service = ctx.GetService<IForecastService>()

             let parameters =
                 GetForecast(
                     Latitude = dto.latitude,
                     Longitude = dto.longitude,
                     TemperatureUnit = dto.temperatureUnit,
                     PrecipitationUnit = dto.precipitationUnit,
                     WindSpeedUnit = dto.windSpeedUnit,
                     Time = dto.time
                 )

             task {
                 let! forecast = service.GetAsync(parameters, tr)

                 let! result = Successful.OK forecast next ctx

                 tr.Finish()

                 return result
             }))
