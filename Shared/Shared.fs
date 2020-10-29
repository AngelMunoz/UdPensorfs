module Shared

open System

type Headers =
    static member PING = "udpensor:ping"
    static member PONG = "udpensor:pong"
    static member FEED = "udpensor:feed"


let getMlsFromSeconds (seconds: int) =
    TimeSpan.FromSeconds(seconds |> float).TotalMilliseconds
    |> int

let getMlsFromMinutes (minutes: int) =
    TimeSpan.FromMinutes(minutes |> float).TotalMilliseconds
    |> int

let private groupArguments (args: seq<string>) =
    args
    |> Seq.skip (2)
    |> Seq.fold (fun (prev: ResizeArray<ResizeArray<string>>) (next: string) ->
        if next.StartsWith('-') then
            let item = ResizeArray()
            item.Add(next)
            prev.Add(item)
        else 
            prev.[prev.Count - 1].Add(next)
        prev) (ResizeArray<ResizeArray<string>>())
    |> List.ofSeq

let parseArguments (args: seq<string>) (defaultPort: int) (defautBroadcastIP: string) =
    let groups = groupArguments args

    let mutable parsed =
        {| ports = List.empty<string>
           bindPort = None
           broadcastIP = None |}

    for group in groups do
        let group = group |> Seq.toList
        if [ "-p"; "--port"; "--ports" ]
           |> List.contains (group.Head) then
            parsed <- {| parsed with ports = group.Tail|}
        if [ "-bp"; "--bindPort"; "--bind-port" ]
           |> List.contains (group.Head) then
            parsed <-
                {| parsed with
                       bindPort = Some(if group.Length > 1 then group.[1] |> int else defaultPort) |}
        if [ "-bip"
             "--broadcastIP"
             "--broadcast-ip" ]
           |> List.contains (group.Head) then
            parsed <-
                {| parsed with
                       broadcastIP = Some(if group.Length > 1 then group.[1] else defautBroadcastIP) |}

    // TODO: add other arguments

    {| parsed with
           bindPort = parsed.bindPort |> Option.defaultValue defaultPort
           broadcastIP =
               parsed.broadcastIP
               |> Option.defaultValue defautBroadcastIP |}
