﻿module TIKSN.hodgepodge.Platform

open System
open Prime

let platformNotFoundError = pair 1469762974 "Platform not Found"

let listProcess () =
    match Environment.OSVersion.Platform with
    | PlatformID.Win32NT -> Ok(Windows.listProcess ())
    | _ -> Error platformNotFoundError
    |> Result.map (fun x -> x |> Seq.map CrossPlatform.setRootProcessParentToNone)
    |> Result.map (fun x -> x |> Seq.toList)

let listServices () =
    match Environment.OSVersion.Platform with
    | PlatformID.Win32NT -> Ok(Windows.listServices ())
    | _ -> Error platformNotFoundError
    |> Result.map (fun x -> x |> Seq.toList)

let listInstalledPrograms () =
    match Environment.OSVersion.Platform with
    | PlatformID.Win32NT -> Ok(Windows.listInstalledPrograms ())
    | _ -> Error platformNotFoundError
    |> Result.map (fun x -> x |> Seq.toList)
