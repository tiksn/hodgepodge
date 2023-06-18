module TIKSN.hodgepodge.CrossPlatform

let makeNullAsNone x =
    match x with
    | Some null -> None
    | _ -> x

let setRootProcessParentToNone (p: ProcessInfo) =
    match p.ParentProcessId with
    | Some pid ->
        if pid = p.ProcessId then
            { p with ParentProcessId = None }
        else
            p
    | None -> p

let groupProcesses (pil: ProcessInfo list) =
    pil |> List.map setRootProcessParentToNone
