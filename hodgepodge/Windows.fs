module TIKSN.hodgepodge.Windows

open System
open System.Management
open Microsoft.Win32
open Prime
open TIKSN.hodgepodge.CrossPlatform

let getManagementObjectPropertyValue<'a> (managementObject: ManagementObject) (name: string) : 'a option =
    try
        Some managementObject[name]
    with _ ->
        None
    |> Option.map (fun x -> x :?> 'a)

let getManagementObjectPropertyRequiredValue<'a> (managementObject: ManagementObject) (name: string) : 'a =
    getManagementObjectPropertyValue<'a> managementObject name |> Option.get

let getManagementObjectPropertyOptionalValue<'a when 'a: null>
    (managementObject: ManagementObject)
    (name: string)
    : 'a option =
    getManagementObjectPropertyValue<'a> managementObject name |> makeNullAsNone

let getRegistryKeyValue<'a> (subKey: RegistryKey) (name: string) : 'a option =
    try
        Some(subKey.GetValue(name))
    with _ ->
        None
    |> Option.map (fun x -> x :?> 'a)

let getRegistryKeyOptionalValue<'a when 'a: null> (subKey: RegistryKey) (name: string) : 'a option =
    getRegistryKeyValue<'a> subKey name |> makeNullAsNone

let listProcess () : ProcessInfo list =
    let processSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_Process")
    let processSearchResult = processSearcher.Get()

    processSearchResult
    |> Seq.cast<ManagementObject>
    |> Seq.map (fun managementObject ->
        { ProcessId = getManagementObjectPropertyRequiredValue<uint> managementObject "ProcessId"
          ParentProcessId = getManagementObjectPropertyValue<uint> managementObject "ParentProcessId"
          Path = getManagementObjectPropertyOptionalValue<string> managementObject "ExecutablePath" })
    |> Seq.toList

let listServices () : ServiceInfo list =
    let serviceSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_Service")
    let serviceSearchResult = serviceSearcher.Get()

    serviceSearchResult
    |> Seq.cast<ManagementObject>
    |> Seq.map (fun managementObject ->
        { ProcessId = getManagementObjectPropertyValue<uint> managementObject "ProcessId"
          Path = getManagementObjectPropertyOptionalValue<string> managementObject "PathName"
          State = getManagementObjectPropertyOptionalValue<string> managementObject "State" })
    |> Seq.toList

let getInstalledProgramForSubKey (key: RegistryKey) (subKeyName: string) : InstalledProgramInfo option =
    let versionStringToEither (versionString: string) : Either<string, Version> =
        let mutable version: Version = null

        match Version.TryParse(versionString, &version) with
        | true -> Right version
        | false -> Left versionString

    let versionOptionalStringToEither (versionString: string option) : Either<string, Version> option =
        match versionString with
        | Some v -> Some(versionStringToEither v)
        | None -> None

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
              Version = (versionOptionalStringToEither displayVersion) }

let listInstalledProgramsForKey
    (hive: RegistryHive)
    (view: RegistryView)
    (registryKey: string)
    : InstalledProgramInfo list =
    use baseKey = RegistryKey.OpenBaseKey(hive, view)
    use key = baseKey.OpenSubKey(registryKey)

    key.GetSubKeyNames()
    |> Array.toSeq
    |> Seq.map (fun x -> getInstalledProgramForSubKey key x)
    |> Seq.filter Option.isSome
    |> Seq.map Option.get
    |> Seq.toList


let listInstalledPrograms () : InstalledProgramInfo list =
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
