module TIKSN.hodgepodge.Windows

open System
open System.Management
open Microsoft.Win32
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

let getRegistryKeyValue<'a> (subKey: RegistryKey) (name: string) =
    try
        Some(subKey.GetValue(name))
    with _ ->
        None
    |> Option.map (fun x -> x :?> 'a)

let getRegistryKeyOptionalValue<'a when 'a: null> (subKey: RegistryKey) (name: string) =
    getRegistryKeyValue<'a> subKey name |> makeNullAsNone

let listProcess () =
    let processSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_Process")
    let processSearchResult = processSearcher.Get()

    processSearchResult
    |> Seq.cast<ManagementObject>
    |> Seq.map (fun managementObject ->
        { ProcessId = getManagementObjectPropertyRequiredValue<uint> managementObject "ProcessId"
          ParentProcessId = getManagementObjectPropertyValue<uint> managementObject "ParentProcessId"
          Path = getManagementObjectPropertyOptionalValue<string> managementObject "ExecutablePath" })
    |> Seq.toList

let listServices () =
    let serviceSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_Service")
    let serviceSearchResult = serviceSearcher.Get()

    serviceSearchResult
    |> Seq.cast<ManagementObject>
    |> Seq.map (fun managementObject ->
        { ProcessId = getManagementObjectPropertyValue<uint> managementObject "ProcessId"
          Path = getManagementObjectPropertyOptionalValue<string> managementObject "PathName"
          State = getManagementObjectPropertyOptionalValue<string> managementObject "State" })
    |> Seq.toList

let getInstalledProgramForSubKey (key: RegistryKey) (subKeyName: string) =
    use subKey = key.OpenSubKey(subKeyName)
    let displayName = getRegistryKeyOptionalValue<string> subKey "DisplayName"
    let installLocation = getRegistryKeyOptionalValue<string> subKey "InstallLocation"
    let displayVersion = getRegistryKeyOptionalValue<string> subKey "DisplayVersion"

    match (displayName, installLocation) with
    | None, None -> None
    | _, _ ->
        Some
            { Name = displayName
              Path = installLocation
              Version = displayVersion }

let listInstalledProgramsForKey (hive: RegistryHive) (view: RegistryView) (registryKey: string) =
    use baseKey = RegistryKey.OpenBaseKey(hive, view)
    use key = baseKey.OpenSubKey(registryKey)

    key.GetSubKeyNames()
    |> Array.toSeq
    |> Seq.map (fun x -> getInstalledProgramForSubKey key x)
    |> Seq.filter Option.isSome
    |> Seq.map Option.get
    |> Seq.toList


let listInstalledPrograms () =
    let uninstallRegistryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"

    let wowUninstallRegistryKey =
        @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall"

    let currentRegistryView =
        if Environment.Is64BitOperatingSystem then
            RegistryView.Registry64
        else
            RegistryView.Registry32

    let installedPrograms =
        (listInstalledProgramsForKey RegistryHive.LocalMachine currentRegistryView uninstallRegistryKey)
        @ (listInstalledProgramsForKey RegistryHive.CurrentUser currentRegistryView uninstallRegistryKey)
        @ (listInstalledProgramsForKey RegistryHive.LocalMachine RegistryView.Registry64 wowUninstallRegistryKey)

    installedPrograms |> List.distinctBy (fun x -> (x.Name, x.Path))
