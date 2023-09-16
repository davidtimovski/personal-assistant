namespace Chef.Utility

[<Measure>] type g
[<Measure>] type oz
[<Measure>] type cup
[<Measure>] type tbsp
[<Measure>] type tsp
[<Measure>] type kg
[<Measure>] type lbs
[<Measure>] type cm
[<Measure>] type inch
[<Measure>] type ft

type IConversion = 
    abstract member ToGrams: unit:string * amount:float32 -> float32
    abstract member ToMilligrams: unit:string * amount:float32 -> float32
    abstract member FeetAndInchesToCentimeters: feet:float32 * inches:float32 -> float32
    abstract member CentimetersToFeetAndInches: centimeters:float32 -> float32 * float32
    abstract member PoundsToKilos: pounds:float32 -> float32
    abstract member KilosToPounds: kilos:float32 -> float32

type Conversion() =
    let gramsPerOunce = 28.3495f<g/oz>
    let gramsPerCup = 220f<g/cup>
    let gramsPerTablespoon = 14.15f<g/tbsp>
    let gramsPerTeaspoon = 4.2f<g/tsp>
    let poundsPerKilo = 2.2046f<lbs/kg>
    let centimetersPerFoot = 30.48f<cm/ft>
    let centimetersPerInch = 2.54f<cm/inch>

    interface IConversion with
        member _.ToGrams(unit: string, amount: float32) =
            match unit with
            | "oz" -> (gramsPerOunce * amount) |> float32
            | "cup" -> (gramsPerCup * amount) |> float32
            | "tbsp" -> (gramsPerTablespoon * amount) |> float32
            | "tsp" -> (gramsPerTeaspoon * amount) |> float32
            | _ -> amount

        member _.ToMilligrams(unit: string, amount: float32) =
            match unit with
            | "oz" -> (gramsPerOunce * amount * 1000f) |> float32
            | "cup" -> (gramsPerCup * amount * 1000f) |> float32
            | "tbsp" -> (gramsPerTablespoon * amount * 1000f) |> float32
            | "tsp" -> (gramsPerTeaspoon * amount * 1000f) |> float32
            | _ -> amount * 1000f

        member _.FeetAndInchesToCentimeters(feet: float32, inches: float32) =
            let feetInCm = (feet * centimetersPerFoot) |> float32
            let inchesInCm = (inches * centimetersPerInch) |> float32
            feetInCm + inchesInCm

        member _.CentimetersToFeetAndInches(centimeters: float32) =
            let totalInches = floor ((centimeters / centimetersPerInch) |> float32)
            let feet = (totalInches - totalInches % 12f) / 12f
            let inches = totalInches % 12f
            (feet, inches)

        member _.PoundsToKilos(pounds: float32) =
            (pounds / poundsPerKilo) |> float32

        member _.KilosToPounds(kilos: float32) =
            let pounds = (kilos * poundsPerKilo) |> float32
            round pounds
