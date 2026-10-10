import { ProviderEnvelope } from '../../IAProviders/DTO/ProviderEnvelope.js';

/**
 * Emisor saliente de mensajes desde el Content Script hacia el Service Worker (Background).
 * Canaliza las respuestas generadas en la página web para ser transmitidas al motor C#.
 */
export class MessageSender {
    constructor() {
        console.log("[MessageSender] 📡 Inicializado emisor saliente hacia el Service Worker.");
    }

    /**
     * Transmite un paquete ProviderEnvelope hacia el Service Worker.
     * @param {ProviderEnvelope} envelope Paquete estricto a emitir.
     * @returns {boolean} Retorna true si se emitió al bus de Chrome.
     */
    send(envelope) {
        if (!(envelope instanceof ProviderEnvelope)) {
            console.error("[MessageSender] ❌ El paquete a enviar debe ser strictly una instancia de ProviderEnvelope.", envelope);
            return false;
        }

        console.log(`[MessageSender] 📤 Despachando a Service Worker -> [Acción: ${envelope.action} | Status: ${envelope.status} | ID: ${envelope.id}]`, envelope);

        try {
            chrome.runtime.sendMessage({
                type: "CS2C_SEND_ENVELOPE",
                envelope: envelope
            }, (response) => {
                if (chrome.runtime.lastError) {
                    console.error("[MessageSender] ❌ Error en el bus de Chrome (chrome.runtime.lastError):", chrome.runtime.lastError.message);
                } else if (response) {
                    console.log("[MessageSender] 📬 ACK / Respuesta recibida del Service Worker:", response);
                }
            });
            return true;
        } catch (err) {
            console.error("[MessageSender] 💥 Excepción crítica emitiendo mensaje a chrome.runtime:", err);
            return false;
        }
    }

    // --- Métodos Helper para Respuestas Rápidas a C# ---

    sendSuccess(action, payload, id = null) {
        console.log(`[MessageSender] 🟢 Preparando envío 'Success' para '${action}'`);
        return this.send(ProviderEnvelope.success(action, payload, id));
    }

    sendProcessing(action, payload = null, id = null) {
        console.log(`[MessageSender] ⏳ Preparando envío 'Processing' para '${action}'`);
        return this.send(ProviderEnvelope.processing(action, payload, id));
    }

    sendLoading(action, payload = null, id = null) {
        console.log(`[MessageSender] 🔄 Preparando envío 'Loading' para '${action}'`);
        return this.send(ProviderEnvelope.loading(action, payload, id));
    }

    sendFail(action, errorMessage, errorCode = 500, id = null) {
        console.warn(`[MessageSender] 🔴 Preparando envío 'Failed' para '${action}': "${errorMessage}" (Código: ${errorCode})`);
        return this.send(ProviderEnvelope.fail(action, errorMessage, errorCode, id));
    }
}