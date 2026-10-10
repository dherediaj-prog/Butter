import { WebSocketClient } from './Entities/Network/WebSocketClients/WebSocketClient.js';
import { ProviderEnvelope } from './Entities/IAProviders/DTO/ProviderEnvelope.js';
import { IPService } from './Services/IPService.js';

const DEFAULT_PORT = 5000;
const WS_PATH = '/ws/chat';
const PING_INTERVAL_MS = 20000; // 20 segundos para mantener con vida al Service Worker

let pingTimer = null;

const wsClient = new WebSocketClient({
    autoReconnect: true,
    reconnectInterval: 3000
});

// Manejador exclusivo para capturar la respuesta PONG del servidor
const backgroundHandlers = {
    "PONG": (envelope) => {
        console.log('[Service Worker] 💓 PONG recibido de C#');
    }
};

function startPingLoop() {
    stopPingLoop();
    pingTimer = setInterval(() => {
        if (wsClient.isConnected) {
            console.log('[Service Worker] 📤 Enviando PING a C#...');
            const pingEnvelope = ProviderEnvelope.processing("PING", { timestamp: Date.now() });
            wsClient.send(pingEnvelope);
        }
    }, PING_INTERVAL_MS);
}

function stopPingLoop() {
    if (pingTimer) {
        clearInterval(pingTimer);
        pingTimer = null;
    }
}

wsClient.onConnected = () => {
    console.log('[Service Worker] 🚀 Conectado con éxito a WinForms C#');
    chrome.action.setBadgeText({ text: "ON" });
    chrome.action.setBadgeBackgroundColor({ color: "#22c55e" });

    // Iniciar emisión recurrente de PING
    startPingLoop();
};

wsClient.onDisconnected = () => {
    stopPingLoop();
    chrome.action.setBadgeText({ text: "OFF" });
    chrome.action.setBadgeBackgroundColor({ color: "#ef4444" });
};

wsClient.onMessage = async (envelope) => {
    const actionKey = envelope.action?.toUpperCase();

    // 1. Si es la respuesta PONG del servidor, la consume el Service Worker
    if (backgroundHandlers[actionKey]) {
        backgroundHandlers[actionKey](envelope);
        return;
    }

    // 2. Si es un comando de IA, se retransmite hacia el Content Script de la pestaña activa
    const [activeTab] = await chrome.tabs.query({ active: true, currentWindow: true });

    if (!activeTab || !activeTab.id) {
        wsClient.sendFail(envelope.action, "No hay ninguna pestaña activa con la IA abierta.", 404, envelope.id);
        return;
    }

    try {
        await chrome.tabs.sendMessage(activeTab.id, {
            type: "C2CS_DISPATCH_ENVELOPE",
            envelope: envelope
        });
    } catch (err) {
        wsClient.sendFail(envelope.action, `Pestaña inalcanzable: ${err.message}`, 500, envelope.id);
    }
};

chrome.runtime.onMessage.addListener((message, sender, sendResponse) => {
    if (message.type === "CS2C_SEND_ENVELOPE") {
        const envelope = ProviderEnvelope.from(message.envelope);
        const delivered = wsClient.send(envelope);
        sendResponse({ ack: true, delivered });
    }

    if (message.type === "GET_STATUS") {
        sendResponse({
            isConnected: wsClient.isConnected,
            url: wsClient.url
        });
    }

    if (message.type === "UPDATE_CONNECTION") {
        const { ip, port } = message;

        if (ip && port) {
            console.log(`[Service Worker] Actualizando conexión a ws://${ip}:${port}${WS_PATH}`);

            chrome.storage.local.set({ serverIp: ip, serverPort: port }, () => {
                const wsUrl = `ws://${ip}:${port}${WS_PATH}`;
                wsClient.connect(wsUrl);
                sendResponse({ success: true, url: wsUrl });
            });

            return true;
        } else {
            sendResponse({ success: false, error: "IP o puerto inválidos" });
        }
    }

    return true;
});

async function startServerConnection() {
    chrome.storage.local.get(['serverIp', 'serverPort'], async (result) => {
        let targetIp = result.serverIp;
        let targetPort = result.serverPort || DEFAULT_PORT;

        if (!targetIp) {
            targetIp = await IPService.getLocalIPAddress();
        }

        const wsUrl = `ws://${targetIp}:${targetPort}${WS_PATH}`;
        console.log(`[Service Worker] Iniciando conexión en: ${wsUrl}`);
        wsClient.connect(wsUrl);
    });
}

startServerConnection();