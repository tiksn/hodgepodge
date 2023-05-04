module TIKSN.hodgepodge.CrossPlatform

let makeNullAsNone x =
    match x with
        | Some null -> None
        | _ -> x
