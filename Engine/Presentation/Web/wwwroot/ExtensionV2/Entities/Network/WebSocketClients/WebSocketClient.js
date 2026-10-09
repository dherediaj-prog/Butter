import { ProviderEnvelope } from '../../IAProviders/DTO/ProviderEnvelope';

/**
 * Cliente WebSocket resiliente enfocado exclusivamente en el protocolo ProviderEnvelope.
 */
export class WebSocketClient {
    /**
     * @param {Object} [options]
     * @param {string} [options.url] URL del WebSocket (ej: "ws://192.168.1.10:5000/ws/chat").
     * @param {boolean} [options.autoReconnect=true] Intenta reconectar automáticamente al perder la conexión.
     * @param {number} [options.reconnectInterval=3000] Tiempo de espera entre reintentos de conexión en ms.
     */
    constructor(options = {}) {
        this.url = options.url || null;
        this.autoReconnect = options.autoReconnect ?? true;
        this.reconnectInterval = options.reconnectInterval || 3000;

        this.socket = null;
        this.isConnected = false;
        this.reconnectTimer = null;

        /** @type {((envelope: ProviderEnvelope, event: MessageEvent) => void) | null} */
        this.onMessage = null;

        this.onConnected = null;
        this.onDisconnected = null;
        this.onError = null;
    }

    /**
     * Establece la conexión con el endpoint de WebSocket.
     * @param {string} [serverUrl] URL opcional si no se pasó en el constructor.
     */
    connect(serverUrl) {
        if (serverUrl) {
            this.url = serverUrl;
        }

        if (!this.url) {
            throw new Error("[WebSocketClient] Se requiere una URL válida (ej. ws://ip:puerto/ws/chat).");
        }

        if (this.socket && (this.socket.readyState === WebSocket.CONNECTING || this.socket.readyState === WebSocket.OPEN)) {
            return;
        }

        this.clearReconnectTimer();

        try {
            this.socket = new WebSocket(this.url);

            this.socket.onopen = (event) => {
                this.isConnected = true;
                console.log(`[WebSocketClient] ✅ Conectado exitosamente a ${this.url}`);

                if (typeof this.onConnected === 'function') {
                    this.onConnected(event);
                }
            };

            this.socket.onmessage = (event) => {
                if (typeof this.onMessage === 'function') {
                    try {
                        // Parsea automáticamente el mensaje entrante hacia una instancia de ProviderEnvelope
                        const envelope = ProviderEnvelope.from(event.data);
                        this.onMessage(envelope, event);
                    } catch (error) {
                        console.error("[WebSocketClient] ❌ Error al deserializar ProviderEnvelope:", error, event.data);
                    }
                }
            };

            this.socket.onerror = (error) => {
                console.error("[WebSocketClient] ❌ Error en la conexión WebSocket:", error);
                if (typeof this.onError === 'function') {
                    this.onError(error);
                }
            };

            this.socket.onclose = (event) => {
                this.isConnected = false;
                console.warn("[WebSocketClient] ⚠️ Conexión cerrada.");

                if (typeof this.onDisconnected === 'function') {
                    this.onDisconnected(event);
                }

                if (this.autoReconnect) {
                    this.scheduleReconnect();
                }
            };
        } catch (error) {
            console.error("[WebSocketClient] Fallo al instanciar el WebSocket:", error);
            if (this.autoReconnect) {
                this.scheduleReconnect();
            }
        }
    }

    /**
     * Envía un paquete ProviderEnvelope al servidor C#.
     * @param {ProviderEnvelope} envelope Instancia estricta de ProviderEnvelope.
     * @returns {boolean} Retorna true si se envió con éxito.
     */
    send(envelope) {
        if (!this.socket || this.socket.readyState !== WebSocket.OPEN) {
            console.error("[WebSocketClient] No se pudo enviar. El socket no está conectado.");
            return false;
        }

        if (!(envelope instanceof ProviderEnvelope)) {
            console.error("[WebSocketClient] ❌ Envío rechazado: El mensaje debe ser estrictamente una instancia de ProviderEnvelope.", envelope);
            return false;
        }

        this.socket.send(JSON.stringify(envelope));
        return true;
    }

    // --- Helpers Convenientes para Responder Directamente a C# ---

    /**
     * Envía un paquete ProviderEnvelope con estado 'Completed'.
     */
    sendSuccess(action, payload, id = null) {
        return this.send(ProviderEnvelope.success(action, payload, id));
    }

    /**
     * Envía un paquete ProviderEnvelope con estado 'Processing'.
     */
    sendProcessing(action, payload = null, id = null) {
        return this.send(ProviderEnvelope.processing(action, payload, id));
    }

    /**
     * Envía un paquete ProviderEnvelope con estado 'Loading'.
     */
    sendLoading(action, payload = null, id = null) {
        return this.send(ProviderEnvelope.loading(action, payload, id));
    }

    /**
     * Envía un paquete ProviderEnvelope con estado 'Failed' y su objeto de error.
     */
    sendFail(action, errorMessage, errorCode = 500, id = null) {
        return this.send(ProviderEnvelope.fail(action, errorMessage, errorCode, id));
    }

    /**
     * Cierra deliberadamente la conexión y desactiva la reconexión automática.
     */
    disconnect() {
        this.autoReconnect = false;
        this.clearReconnectTimer();

        if (this.socket) {
            this.socket.close();
            this.socket = null;
        }

        this.isConnected = false;
    }

    // --- Privados / Auxiliares ---

    scheduleReconnect() {
        this.clearReconnectTimer();
        console.log(`[WebSocketClient] Reintentando conexión en ${this.reconnectInterval / 1000}s...`);
        this.reconnectTimer = setTimeout(() => {
            this.connect();
        }, this.reconnectInterval);
    }

    clearReconnectTimer() {
        if (this.reconnectTimer) {
            clearTimeout(this.reconnectTimer);
            this.reconnectTimer = null;
        }
    }
}