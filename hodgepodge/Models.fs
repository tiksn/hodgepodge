namespace TIKSN.hodgepodge

type ProcessInfo =
    { ProcessId: uint
      ParentProcessId: uint option
      Path: string option }

type ServiceInfo =
    { ProcessId: uint option
      Path: string option
      State: string option }

type InstalledProgramInfo =
    { Name: string option
      Path: string option
      Version: string option }
