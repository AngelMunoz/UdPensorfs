module Client

open Fable.Core.JS
open Node.Api
open Shared

let args =
    parseArguments ``process``.argv 7777 "255.255.255.255"

let client = dgram.createSocket "udp4"

let servers =
    System.Collections.Generic.Dictionary<string, {| address: string; port: int |}>()

client.on("connect",
          (fun _ ->
              for address in servers.Values do
                  printfn "Connected to remote: [%s:%i]" address.address address.port

                  let msg =
                      sprintf "%s| sending sensor readings..." Headers.FEED

                  let id =
                      setInterval (fun _ -> client.send (msg, 0, msg.Length) |> ignore) 2000

                  setTimeout (fun _ -> clearInterval (id)) 20000
                  |> ignore))
      .on("listening",
          (fun _ ->
              let address = client.address ()
              printfn "Server listening at %s - %s:%i" address.family address.address address.port))
      .on("message",
          (fun (buffer: Node.Buffer.Buffer) (info: Node.Dgram.AddressInfo) ->
              let text = buffer.toString ()
              let address = info.address
              let port = info.port
              if text = Headers.PING then
                  if not (servers.ContainsKey address) then
                      servers.Add(address, {| address = address; port = port |})
                      let msg = sprintf "%s| I see you..." Headers.PONG

                      let cb (error: Node.Base.Error): unit =
                          let error = Option.ofObj error
                          match error with
                          | Some error -> printfn "%O" error
                          | None ->
                              // Completely optional since you already have this server on the "servers" map
                              client.connect (port, address)
                          |> ignore

                      client.send (msg, 0, msg.Length, port = info.port, address = info.address, callback = cb)
                      |> ignore
              else
                  // handle unknown messages
                  printfn "Got [%s] from %s:%i" text info.address info.port)).on("close", (fun _ -> printfn "Closing"))
      .on("error", (fun error -> printfn "%O" error))
|> ignore



// Bind port and
client.bind (port = args.bindPort, callback = fun () -> client.setBroadcast (true))
