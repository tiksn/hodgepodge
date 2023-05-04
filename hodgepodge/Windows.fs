namespace TIKSN.hodgepodge

open System.Management

module Windows =
    let getManagementObjectPropertyValue<'a> (managementObject: ManagementObject) (name: string) =
        try
            Some managementObject[name]
        with _ ->
            None
        |> Option.map (fun x -> x :?> 'a)

    let getManagementObjectPropertyRequiredValue<'a> (managementObject: ManagementObject) (name: string) =
        getManagementObjectPropertyValue<'a> managementObject name |> Option.get

    let listProcess () =
        let processSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_Process")
        let processSearchResult = processSearcher.Get()

        processSearchResult
        |> Seq.cast<ManagementObject>
        |> Seq.map (fun managementObject ->
            { ProcessId = getManagementObjectPropertyRequiredValue<uint> managementObject "ProcessId"
              ParentProcessId = getManagementObjectPropertyValue<uint> managementObject "ParentProcessId"
              Path = getManagementObjectPropertyValue<string> managementObject "ExecutablePath" })
