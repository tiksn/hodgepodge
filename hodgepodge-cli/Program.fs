open TIKSN.hodgepodge

let processes = Platform.listProcess ()

match processes with
| Ok v -> Dumpify.DumpExtensions.Dump(v) |> ignore
| Error _ -> printfn "Failed to get process list"
