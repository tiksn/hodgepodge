module TIKSN.hodgepodge.Platform

open System
open Prime

let platformNotFoundError = pair 1469762974 "Platform not Found"

let listProcess () : Result<Either<ProcessInfo, ProcessInfo list> list, (int * string)> =
    match Environment.OSVersion.Platform with
    | PlatformID.Win32NT -> Ok(Windows.listProcess ())
    | _ -> Error platformNotFoundError
    |> Result.map CrossPlatform.groupProcesses

let listServices () : Result<ServiceInfo list, (int * string)> =
    match Environment.OSVersion.Platform with
    | PlatformID.Win32NT -> Ok(Windows.listServices ())
    | _ -> Error platformNotFoundError

let listInstalledPrograms () : Result<InstalledProgramInfo list, (int * string)> =
    match Environment.OSVersion.Platform with
    | PlatformID.Win32NT -> Ok(Windows.listInstalledPrograms ())
    | _ -> Error platformNotFoundError
