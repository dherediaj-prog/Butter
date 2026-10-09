import { WebSocketClient } from './Entities/Network/WebSocketClients/WebSocketClient.js';
import { ProviderEnvelope } from './Entities/IAProviders/DTO/ProviderEnvelope.js';
import { IPService } from './Services/IPService.js';

const PORT = 5000;
const WS_PATH = '/ws/chat';

const wsClient = new WebSocketClient({
    autoReconnect: true,
    reconnectInterval: 3000
});

wsClient.onConnected = () => {
    console.log('[Service Worker] 🚀 Conectado con éxito a WinForms C#');
    // Indicador verde en el ícono de la extensión
    chrome.action.setBadgeText({ text: "ON" });
    chrome.action.setBadgeBackgroundColor({ color: "#22c55e" });
};

wsClient.onDisconnected = () => {
    // Indicador rojo en el ícono de la extensión
    chrome.action.setBadgeText({ text: "OFF" });
    chrome.action.setBadgeBackgroundColor({ color: "#ef4444" });
};

wsClient.onMessage = async (envelope) => {
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
        wsClient.send(envelope);
    }

    // Consulta de estado desde el Popup UI
    if (message.type === "GET_STATUS") {
        sendResponse({
            isConnected: wsClient.isConnected,
            url: wsClient.url
        });
    }
});

async function startServerConnection() {
    const localIp = await IPService.getLocalIPAddress();
    const wsUrl = `ws://${localIp}:${PORT}${WS_PATH}`;
    wsClient.connect(wsUrl);
}

startServerConnection();