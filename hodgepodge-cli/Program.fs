open TIKSN.hodgepodge

let processes = Platform.listProcess ()
let services = Platform.listServices ()

match services with
| Ok v -> Dumpify.DumpExtensions.Dump(v) |> ignore
| Error _ -> printfn "Failed to get process list"
