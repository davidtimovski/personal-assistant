module ForecastHandlers

open Giraffe
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
open Weatherman.Application.Contracts.Forecasts
open Weatherman.Application.Contracts.Forecasts.Models
open Models

let successOrLog (handler: HttpHandler) (next: HttpFunc) (ctx: HttpContext) =
    task {
        try
            return! handler next ctx
        with ex ->
            let logger = ctx.GetService<ILogger<HttpContext>>()
            logger.LogError(ex, $"Unexpected error in handler for request: {ctx.Request.Method} {ctx.Request.Path}")

            return! ServerErrors.INTERNAL_ERROR "An unexpected error occurred" next ctx
    }

let get: HttpHandler =
    successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
        let result = ctx.TryBindQueryString<GetForecastDto>()

        (match result with
            | Error _ -> RequestErrors.BAD_REQUEST "Bad request" next ctx
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
                    let! forecast = service.GetAsync(parameters)

                    return! Successful.OK forecast next ctx
                }
        )
    )
