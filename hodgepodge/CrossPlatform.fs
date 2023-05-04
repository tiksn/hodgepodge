namespace TIKSN.hodgepodge

open System
open Prime

module CrossPlatform =
    let listProcess () =
        match Environment.OSVersion.Platform with
        | PlatformID.Win32NT -> Ok(Windows.listProcess ())
        | _ -> Error(pair 1469762974 "Platform not Found")
        |> Result.map (fun x -> x |> Seq.toList)
        
        
