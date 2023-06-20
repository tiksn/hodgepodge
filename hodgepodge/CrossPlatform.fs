module TIKSN.hodgepodge.CrossPlatform

open Prime

let makeNullAsNone (x: 'a option) : 'a option =
    match x with
    | Some null -> None
    | _ -> x

let setRootProcessParentToNone (p: ProcessInfo) : ProcessInfo =
    match p.ParentProcessId with
    | Some pid ->
        if pid = p.ProcessId then
            { p with ParentProcessId = None }
        else
            p
    | None -> p

let findProcessClusters (list: ProcessInfo list) : Either<ProcessInfo, ProcessTreeInfo> list =
    let shouldBeClusterParent (item: ProcessInfo) (cluster: ProcessTreeInfo) : bool =
        item.Path.IsSome
        && item.Path = cluster.Path
        && (Some item.ProcessId = cluster.ParentProcessId)

    let shouldBeClusterChild (item: ProcessInfo) (cluster: ProcessTreeInfo) : bool =
        item.Path.IsSome
        && item.Path = cluster.Path
        && (Some cluster.ProcessId = item.ParentProcessId)

    let rec addToCluster (item: ProcessInfo) (cluster: ProcessTreeInfo) : ProcessTreeInfo option =
        if shouldBeClusterParent item cluster then
            Some
                { ProcessId = item.ProcessId
                  ParentProcessId = item.ParentProcessId
                  Path = item.Path
                  Children = [ cluster ] }
        elif shouldBeClusterChild item cluster then
            Some
                { cluster with
                    Children =
                        ({ ProcessId = item.ProcessId
                           ParentProcessId = item.ParentProcessId
                           Path = item.Path
                           Children = [] }
                         :: cluster.Children) }
        else
            match cluster.Children with
            | [] -> None
            | _ ->
                (let changedChildren =
                    cluster.Children
                    |> List.map (fun x ->
                        let newChild = addToCluster item x

                        match newChild with
                        | Some c ->
                            { cluster with
                                Children = (c :: cluster.Children) }
                        | None -> x)

                 if cluster.Children <> changedChildren then
                     Some
                         { cluster with
                             Children = changedChildren }
                 else
                     None)

    let mapProcessInfoToCluster (pi: ProcessInfo) : ProcessTreeInfo =
        { ProcessId = pi.ProcessId
          ParentProcessId = pi.ParentProcessId
          Path = pi.Path
          Children = [] }

    let mapClusterToEither (cluster: ProcessTreeInfo) : Either<ProcessInfo, ProcessTreeInfo> =
        match cluster.Children with
        | [] ->
            Left
                { ProcessId = cluster.ProcessId
                  ParentProcessId = cluster.ParentProcessId
                  Path = cluster.Path }
        | _ -> Right cluster

    let rec clusterHelper
        (acc: Either<ProcessInfo, ProcessTreeInfo> list)
        (currentCluster: ProcessTreeInfo)
        (rest: ProcessInfo list)
        : Either<ProcessInfo, ProcessTreeInfo> list =
        match rest with
        | [] -> mapClusterToEither (currentCluster) :: acc
        | x :: xs ->
            let newCluster = addToCluster x currentCluster

            match newCluster with
            | Some c -> clusterHelper acc c xs
            | None -> clusterHelper (mapClusterToEither (currentCluster) :: acc) (mapProcessInfoToCluster x) xs

    match list with
    | [] -> []
    | x :: xs -> List.rev (clusterHelper [] (mapProcessInfoToCluster x) xs)

let groupProcesses (pil: ProcessInfo list) : Either<ProcessInfo, ProcessTreeInfo> list =
    pil |> List.map setRootProcessParentToNone |> findProcessClusters
