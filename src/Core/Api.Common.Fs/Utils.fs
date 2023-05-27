module Utils

let noneOrTrimmed (text: string Option) =
    match text with
    | None -> None
    | Some t ->
        let trimmed = t.Trim()
        if trimmed = "" then None else Some(trimmed)
