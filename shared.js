import { parse } from "path";

export const headers = {
    PING: "udpensor:ping",
    PONG: "udpensor:pong",
    FEED: "udpensor:feed"
}

/**
 * 
 * @param {number} seconds 
 */
export const getSeconds = seconds => seconds * 1000;

/**
 * 
 * @param {number} minutes 
 */
export const getMinutes = minutes => getSeconds(minutes * 60);

/**
 * 
 * @param  {string[]} args 
 */
export const groupArguments = (args) => {
    return args.reduce((prev, next, i) => {
        if (i < 2) return prev;
        if (next.startsWith('-')) {
            prev.push([next]);
            return prev;
        }
        prev[prev.length - 1].push(next);
        return prev;
    }, []);
}

/**
 * 
 * @param {string[][]} groups 
 * @param {number} defaultPort
 */
export const parseArguments = (args, defaultPort) => {
    const groups = groupArguments(args);
    const parsed = {
        ports: [],
        bindPort: -1,
        broadcastIP: ""
    }
    for (const group of groups) {
        if (["-p", "--port", "--ports"].includes(group[0])) {
            parsed.ports.push(...group.slice(1));
        }
        if (["-bp", "--bind-port"].includes(group[0])) {
            parsed.bindPort = group.length > 1 ? group[1] : defaultPort;
        }
        if (["-bip", "--broadcast-ip"].includes(group[0])) {
            parsed.broadcastIP = group.length > 1 ? group[1] : "255.255.255.255";
        }
        // TODO: add other arguments
    }
    if (parsed.bindPort === -1) { parsed.bindPort = defaultPort; }
    return parsed;
}