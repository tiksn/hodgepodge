module TIKSN.hodgepodge.CrossPlatform

let makeNullAsNone x =
    match x with
    | Some null -> None
    | _ -> x

let setRootProcessParentToNone (p: ProcessInfo) =
    match p.ParentProcessId with
    | Some pid ->
        if pid = p.ProcessId then
            { ProcessId = p.ProcessId
              ParentProcessId = None
              Path = p.Path }
        else
            p
    | None -> p
