open Spectre.Console
open TIKSN.hodgepodge

let logError (result: Result<'a, (int * string)>) (action: string) : unit =
    match result with
    | Ok _ -> ()
    | Error errorValue ->
        let errorCode, errorMessage = errorValue
        AnsiConsole.MarkupLineInterpolated($"[purple]{errorCode}[/]: [red]{errorMessage}[/] at [blue]{action}[/]")

    ()

let search (ctx: StatusContext) : unit =
    ctx.Status <- "Listing Processes..."
    let processes = Platform.listProcess ()
    logError processes "Listing Processes"

    ctx.Status <- "Listing Services..."
    let services = Platform.listServices ()
    logError services "Listing Services"


    ctx.Status <- "Listing Installed Programs..."
    let installedPrograms = Platform.listInstalledPrograms ()
    logError installedPrograms "Listing Installed Programs"

    ()

AnsiConsole.Status().Start("Searching...", search)
