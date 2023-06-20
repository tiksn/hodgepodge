module TIKSN.hodgepodge.Platform

open System
open Prime

let platformNotSupportedError = pair 1469762974 "Platform is not supported"

let listProcess () : Result<Either<ProcessInfo, ProcessTreeInfo> list, (int * string)> =
    match Environment.OSVersion.Platform with
    | PlatformID.Win32NT -> Ok(Windows.listProcess ())
    | _ -> Error platformNotSupportedError
    |> Result.map CrossPlatform.groupProcesses

let listServices () : Result<ServiceInfo list, (int * string)> =
    match Environment.OSVersion.Platform with
    | PlatformID.Win32NT -> Ok(Windows.listServices ())
    | _ -> Error platformNotSupportedError

let listInstalledPrograms () : Result<InstalledProgramInfo list, (int * string)> =
    match Environment.OSVersion.Platform with
    | PlatformID.Win32NT -> Ok(Windows.listInstalledPrograms ())
    | _ -> Error platformNotSupportedError
