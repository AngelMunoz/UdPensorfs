module Server

open Fable.Core.JS
open Node
open Node.Api
open Shared

let args =
    parseArguments ``process``.argv 16500 "255.255.255.255"

let server = dgram.createSocket "udp4"
let BROADCAST_IP = args.broadcastIP

let clients =
    System.Collections.Generic.Dictionary<string, {| address: string; port: int |}>()


let checkPort (server: Dgram.Socket) (port: int) (interval: int) =
    setInterval (fun _ ->
        printfn "Sending [%s] to %s:%i" Headers.PING BROADCAST_IP port
        server.send (Headers.PING, 0, Headers.PING.Length, port = port, address = BROADCAST_IP)
        |> ignore) interval

let discoverNetwork (server: Dgram.Socket) (knownPorts: seq<int>) (interval: int) (timeout: int) =
    printfn "Ping Network in %i ports" (knownPorts |> Seq.length)
    for port in knownPorts do
        let intervalId = checkPort server port interval
        setTimeout (fun _ -> clearInterval intervalId) timeout
        |> ignore

let feedData (data: string) =
    let data = data.Substring(Headers.FEED.Length)
    printfn "Feeding [%s]" data

server.on("listening",
          (fun _ ->
              let address = server.address ()
              printfn "Server listening at %s - %s:%i" address.family address.address address.port
              server.setBroadcast (true)
              let ports = args.ports |> List.map int
              discoverNetwork server ports (getMlsFromSeconds 10) (getMlsFromMinutes 2)))
      .on("message",
          (fun (buffer: Node.Buffer.Buffer) (info: Node.Dgram.AddressInfo) ->
              let text = buffer.toString ()
              let address = info.address
              let port = info.port
              match text with
              | msg when msg = Headers.PONG ->
                  if (not (clients.ContainsKey address)) then
                      clients.Add(address, {| address = address; port = port |})
                      // TODO: Add interface to multicast if you cant to notify specific clients
                      // Or simply use the map to pull the clients
                      printfn "Got PONG msg [%s] from %s:%i" text address port
              | msg when msg.StartsWith(Headers.FEED) -> feedData (text)
              | _ ->
                  // All unknown messages get here
                  printfn """Got: [%s] from %s:%i """ text address port)).on("error", (fun err -> eprintfn "%O" err))
|> ignore

server.bind (args.bindPort)
