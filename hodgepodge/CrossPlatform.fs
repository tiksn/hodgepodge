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

let findProcessClusters (list: ProcessInfo list) : Either<ProcessInfo, ProcessInfo list> list =
    let rec shouldBeInCluster (item: ProcessInfo) (cluster: ProcessInfo list) : bool =
        match cluster with
        | [] -> false
        | x :: xs ->
            if
                item.Path.IsSome
                && item.Path = x.Path
                && (Some x.ProcessId = item.ParentProcessId
                    || x.ParentProcessId = Some item.ProcessId)
            then
                true
            else
                shouldBeInCluster item xs

    let rec clusterHelper
        (acc: ProcessInfo list list)
        (currentCluster: ProcessInfo list)
        (rest: ProcessInfo list)
        : ProcessInfo list list =
        match rest with
        | [] -> currentCluster :: acc
        | x :: xs ->
            if shouldBeInCluster x currentCluster then
                clusterHelper acc (x :: currentCluster) xs
            else
                clusterHelper (currentCluster :: acc) [ x ] xs

    let mapClusterToEitherList (cluster: ProcessInfo list) : Either<ProcessInfo, ProcessInfo list> =
        match cluster with
        | [ x ] -> Left x
        | _ -> Right cluster

    let clusters =
        match list with
        | [] -> []
        | x :: xs -> List.rev (clusterHelper [] [ x ] xs)

    clusters |> List.map mapClusterToEitherList

let groupProcesses (pil: ProcessInfo list) : Either<ProcessInfo, ProcessInfo list> list =
    pil |> List.map setRootProcessParentToNone |> findProcessClusters
