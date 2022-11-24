module Models

open System

type CreateError =
    {
        Application: string
        Message : string
        StackTrace : string
        Occurred : DateTime
    }
