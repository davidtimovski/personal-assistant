module Models

open System

type CreateError =
    {
        UserId: int
        Application: string
        Message : string
        StackTrace : string
        Occurred : DateTime
    }
