module Models

open System

type CreateError =
    {
        UserId : int
        Message : string
        StackTrace : string
        Occurred : DateTime
    }
