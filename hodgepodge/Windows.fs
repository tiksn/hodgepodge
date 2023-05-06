module TIKSN.hodgepodge.Windows

open System.Management
open TIKSN.hodgepodge.CrossPlatform

let getManagementObjectPropertyValue<'a> (managementObject: ManagementObject) (name: string) =
    try
        Some managementObject[name]
    with _ ->
        None
    |> Option.map (fun x -> x :?> 'a)

let getManagementObjectPropertyRequiredValue<'a> (managementObject: ManagementObject) (name: string) =
    getManagementObjectPropertyValue<'a> managementObject name |> Option.get

let getManagementObjectPropertyOptionalValue<'a when 'a: null> (managementObject: ManagementObject) (name: string) =
    getManagementObjectPropertyValue<'a> managementObject name |> makeNullAsNone

let listProcess () =
    let processSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_Process")
    let processSearchResult = processSearcher.Get()

    processSearchResult
    |> Seq.cast<ManagementObject>
    |> Seq.map (fun managementObject ->
        { ProcessId = getManagementObjectPropertyRequiredValue<uint> managementObject "ProcessId"
          ParentProcessId = getManagementObjectPropertyValue<uint> managementObject "ParentProcessId"
          Path = getManagementObjectPropertyOptionalValue<string> managementObject "ExecutablePath" })

let listServices () =
    let serviceSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_Service")
    let serviceSearchResult = serviceSearcher.Get()

    serviceSearchResult
    |> Seq.cast<ManagementObject>
    |> Seq.map (fun managementObject ->
        { ProcessId = getManagementObjectPropertyValue<uint> managementObject "ProcessId"
          Path = getManagementObjectPropertyOptionalValue<string> managementObject "PathName"
          State = getManagementObjectPropertyOptionalValue<string> managementObject "State" })
