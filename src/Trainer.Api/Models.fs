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
type AmountSeries =
    { serie: int
      amount: int }

[<CLIMutable>]
type CreateProgressEntry =
    { exerciseId: int
      date: DateOnly
      series: seq<AmountSeries> }
