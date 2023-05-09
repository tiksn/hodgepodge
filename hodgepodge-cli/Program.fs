open TIKSN.hodgepodge

let processes = Platform.listProcess ()
let services = Platform.listServices ()
let installedPrograms = Platform.listInstalledPrograms ()

match installedPrograms with
| Ok v -> Dumpify.DumpExtensions.Dump(v) |> ignore
| Error _ -> printfn "Failed to get process list"
