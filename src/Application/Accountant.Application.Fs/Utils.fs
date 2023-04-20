namespace Accountant.Application.Fs

module Utils =
    let noneOrTrimmed (text: string Option) =
        match text with
        | None -> None
        | Some t -> if t.Trim() = "" then None else Some(t.Trim())
