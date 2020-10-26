import { createSocket } from 'dgram';
import { headers, parseArguments } from './shared.js';

const args = parseArguments(process.argv, 7777);

const client = createSocket('udp4');
const servers = new Map();
client
    .on('connect', () => {
        for (const [_, { address, port }] of servers) {
            console.log(`Connected to Remote [${address}:${port}]`);
            const id = setInterval(() => client.send(`${headers.FEED}| sending sensor readings...`), 2000)
            setTimeout(() => clearInterval(id), 20000);
        }
    })
    .on('listening', () => {
        const address = client.address();
        console.log(`Server listening at ${address.family} - ${address.address}:${address.port}`);
    })
    .on('message', (buffer, info) => {
        const text = buffer.toString();
        if (text === headers.PING) {
            const { address, port } = info
            if (servers.has(address)) { return; }
            servers.set(address, { address, port });
            client.send(`${headers.PONG}| I see you`, info.port, info.address, (error, bytes) => {
                if (error) { console.error(error); return; }
                // Completely optional since you already have this server on the "servers" map
                client.connect(port, address);
            });
            return;
        }
        // handle unknown messages
        console.log(`Got [${text}] from ${info.address}:${info.port}`);
    })
    .on('close', () => {
        console.error("Closing");
    })
    .on('error', error => {
        console.error("error", error);
    });



// Bind port and 
client.bind(args.bindPort, () => client.setBroadcast(true));