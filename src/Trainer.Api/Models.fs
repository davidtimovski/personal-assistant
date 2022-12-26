module Models

open System

type ExerciseType =
    | Amount = 0
    | AmountX2 = 1

[<CLIMutable>]
type EditAmountExercise =
    { id: int
      name: string
      sets: int
      amountUnit: string
      ofType: ExerciseType }

[<CLIMutable>]
type EditAmountX2Exercise =
    { id: int
      name: string
      sets: int
      amount1Unit: string
      amount2Unit: string
      ofType: ExerciseType }

[<CLIMutable>]
type CreateAmountExercise =
    { name: string
      sets: int
      amountUnit: string }

[<CLIMutable>]
type CreateAmountX2Exercise =
    { name: string
      sets: int
      amount1Unit: string
      amount2Unit: string }

[<CLIMutable>]
type AmountSet =
    { set: int
      amount: int }

[<CLIMutable>]
type CreateProgressAmount =
    { exerciseId: int
      date: DateOnly
      sets: seq<AmountSet> }

[<CLIMutable>]
type EditProgressAmount =
    { exerciseId: int
      date: DateOnly
      sets: seq<AmountSet> }

[<CLIMutable>]
type AmountX2Set =
    { set: int
      amount1: int
      amount2: int }

[<CLIMutable>]
type EditProgressAmountX2 =
    { exerciseId: int
      date: DateOnly
      sets: seq<AmountX2Set> }
