module Models

type ValidationFailure =
    { Field: string
      ErrorMessage: string }

type Result<'TSuccess> =
    | Success of 'TSuccess
    | Failure of ValidationFailure

let bind switchFunction twoTrackInput =
    match twoTrackInput with
    | Success s -> switchFunction s
    | Failure f -> Failure f
