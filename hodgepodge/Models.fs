namespace TIKSN.hodgepodge

open System
open Prime

type ProcessInfo =
    { ProcessId: uint
      ParentProcessId: uint option
      Path: string option }

type ProcessTreeInfo =
    { ProcessId: uint
      ParentProcessId: uint option
      Path: string option
      Children: ProcessTreeInfo list }

type ServiceInfo =
    { ProcessId: uint option
      Path: string option
      State: string option }

type InstalledProgramInfo =
    { Name: string option
      Path: string option
      Version: Either<string, Version> option }
