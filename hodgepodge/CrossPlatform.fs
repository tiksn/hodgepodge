module TIKSN.hodgepodge.CrossPlatform

open Prime

let makeNullAsNone x =
    match x with
    | Some null -> None
    | _ -> x

let setRootProcessParentToNone (p: ProcessInfo) =
    match p.ParentProcessId with
    | Some pid ->
        if pid = p.ProcessId then
            { p with ParentProcessId = None }
        else
            p
    | None -> p

let findProcessClusters (list: ProcessInfo list) =
    let rec shouldBeInCluster (item: ProcessInfo) (cluster: ProcessInfo list) =
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

    let rec clusterHelper (acc: ProcessInfo list list) currentCluster rest =
        match rest with
        | [] -> currentCluster :: acc
        | x :: xs ->
            if shouldBeInCluster x currentCluster then
                clusterHelper acc (x :: currentCluster) xs
            else
                clusterHelper (currentCluster :: acc) [ x ] xs

    let mapClusterToEitherList (cluster: ProcessInfo list) =
        match cluster with
        | [ x ] -> Left x
        | _ -> Right cluster

    let clusters =
        match list with
        | [] -> []
        | x :: xs -> List.rev (clusterHelper [] [ x ] xs)

    clusters |> List.map mapClusterToEitherList

let groupProcesses (pil: ProcessInfo list) =
    pil |> List.map setRootProcessParentToNone |> findProcessClusters
