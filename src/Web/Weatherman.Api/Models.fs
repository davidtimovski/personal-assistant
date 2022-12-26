module Models

open System

[<CLIMutable>]
type GetForecastDto =
    { latitude: float32
      longitude: float32
      temperatureUnit: string
      precipitationUnit: string
      windSpeedUnit: string
      time: DateTime }
