import {ProviderEnvelope} from '../../IAProviders/DTO/ProviderEnvelope';

/**
 * Emisor saliente de mensajes desde el Content Script hacia el Service Worker (Background).
 * Canaliza las respuestas generadas en la página web para ser transmitidas al motor C#.
 */
export class MessageSender {
    /**
     * Transmite un paquete ProviderEnvelope hacia el Service Worker.
     * @param {ProviderEnvelope} envelope Paquete estricto a emitir.
     * @returns {boolean} Retorna true si se emitió al bus de Chrome.
     */
    send(envelope) {
        if (!(envelope instanceof ProviderEnvelope)) {
            console.error("[MessageSender] ❌ El paquete a enviar debe ser estrictamente una instancia de ProviderEnvelope.", envelope);
            return false;
        }

        chrome.runtime.sendMessage({
            type: "CS2C_SEND_ENVELOPE",
            envelope: envelope
        });
        return true;
    }

    // --- Métodos Helper para Respuestas Rápidas a C# ---

    sendSuccess(action, payload, id = null) {
        return this.send(ProviderEnvelope.success(action, payload, id));
    }

    sendProcessing(action, payload = null, id = null) {
        return this.send(ProviderEnvelope.processing(action, payload, id));
    }

    sendLoading(action, payload = null, id = null) {
        return this.send(ProviderEnvelope.loading(action, payload, id));
    }

    sendFail(action, errorMessage, errorCode = 500, id = null) {
        return this.send(ProviderEnvelope.fail(action, errorMessage, errorCode, id));
    }
}