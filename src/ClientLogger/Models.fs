module Models

open System

type LogError =
    {
        UserId : int
        Message : string
        StackTrace : string
        Occurred : DateTime
    }
