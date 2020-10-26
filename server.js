//@ts-check
import dgram from 'dgram';
import {
    headers,
    getSeconds,
    getMinutes,
    parseArguments
} from './shared.js';

const args = parseArguments(process.argv, 16500);

const server = dgram.createSocket('udp4');
const BROADCAST_IP = args.broadcastIP;
const registeredAddreses = new Map();

/**
 * 
 * @param {import('dgram').Socket} server 
 * @param {number} port
 * @param {number} interval
 */
function checkPort(server, port, interval) {
    return setInterval(() => {
        console.log(`Sending [${headers.PING}] to ${BROADCAST_IP}:${port}`);
        server.send(headers.PING, port, BROADCAST_IP);
    }, interval)
}

/**
 * 
 * @param {import('dgram').Socket} server
 * @param {number[]} knownPorts 
 * @param {number} interval 
 * @param {number} timeout 
 */
function discoverNetwork(server, knownPorts, interval, timeout) {
    console.log(`Discover clients on Network listening in ${knownPorts.length} ports`)
    for (const port of knownPorts) {
        const intervalId = checkPort(server, port, interval)
        setTimeout(() => {
            clearInterval(intervalId);
        }, timeout);
    }
}

/**
 * 
 * @param {string} dataStr 
 */
function feedData(dataStr) {
    const data = dataStr.slice(headers.FEED.length);
    console.log(`Feeding ${data}`);
}

server
    .on('listening', () => {
        const address = server.address()
        console.log(`Server listening at ${address.family} - ${address.address}:${address.port}`);
        server.setBroadcast(true);
        discoverNetwork(server, args.ports, getSeconds(5), getMinutes(2));
    })
    .on('message', (buffer, info) => {
        const text = buffer.toString();
        if (text === headers.PONG) {
            const { address, port } = info;
            if (registeredAddreses.has(address)) { return; }
            registeredAddreses.set(address, { address, port });
            // TODO: Add interface to multicast if you cant to notify specific clients
            // Or simply use the map to pull the clients
            console.log(`Got pong msg: [${text}] from "${info.address}:${info.port}"`);
        }
        if (text.includes(headers.FEED)) {
            feedData(text);
            return;
        }
        // All unknown messages get here
        console.log(`Got: "${text}" from "${info.address}:${info.port}"`);
    })
    .on('error', err => {
        console.error("error", err);
    });

server.bind(args.bindPort);


