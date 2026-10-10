import { ProviderEnvelope } from '../../IAProviders/DTO/ProviderEnvelope.js';
import { IClientMessageHandler } from './Interfaces/IClientMessageHandler.js';

/**
 * Despachador de mensajes del Content Script.
 * Escucha mensajes emitidos desde el Service Worker (C2CS_DISPATCH_ENVELOPE)
 * y delega la ejecución de comandos al manejador (handler) correspondiente.
 */
export class MessageDispatcher {
    /**
     * @param {Object} [client=null] Puente de extensión (ExtensionClientBridge) para responder hacia C#.
     * @param {IClientMessageHandler[]} [handlers=[]] Lista opcional de handlers iniciales.
     */
    constructor(client = null, handlers = []) {
        this.client = client;
        /** @type {Map<string, IClientMessageHandler>} */
        this.handlers = new Map();

        handlers.forEach(handler => this.registerHandler(handler));

        this.listen();
    }

    /**
     * Registra un manejador de acción.
     * @param {IClientMessageHandler} handler
     */
    registerHandler(handler) {
        if (!handler || !handler.action) {
            throw new Error("[MessageDispatcher] El manejador debe definir una propiedad 'action' válida.");
        }

        this.handlers.set(handler.action.toUpperCase(), handler);
        return this;
    }

    /**
     * Permite actualizar o reasignar el cliente/puente de respuesta.
     * @param {Object} client
     */
    setClient(client) {
        this.client = client;
    }

    /**
     * Se suscribe al bus de mensajes nativo de Chrome (`chrome.runtime.onMessage`).
     * Filtra los eventos dirigidos a la acción 'C2CS_DISPATCH_ENVELOPE'.
     */
    listen() {
        if (typeof chrome !== 'undefined' && chrome.runtime?.onMessage) {
            chrome.runtime.onMessage.addListener((message) => {
                if (message?.type === "C2CS_DISPATCH_ENVELOPE" && message.envelope) {
                    const envelope = ProviderEnvelope.from(message.envelope);
                    this.dispatch(envelope);
                }
            });
        }
    }

    /**
     * Despacha un ProviderEnvelope entrante hacia el handler correspondiente.
     * @param {ProviderEnvelope} envelope
     */
    async dispatch(envelope) {
        if (!envelope || !envelope.action) {
            this.client?.sendFail("UNKNOWN", "La acción recibida no puede estar vacía", 400, envelope?.id);
            return;
        }

        const actionKey = envelope.action.toUpperCase();
        const handler = this.handlers.get(actionKey);

        if (handler) {
            try {
                await handler.handle(this.client, envelope);
            } catch (error) {
                console.error(`[MessageDispatcher] ❌ Error procesando '${envelope.action}':`, error);
                this.client?.sendFail(
                    envelope.action,
                    `Error interno al procesar la acción: ${error.message}`,
                    500,
                    envelope.id
                );
            }
        } else {
            console.warn(`[MessageDispatcher] ⚠️ Acción sin handler registrado: '${envelope.action}'`);
            this.client?.sendFail(
                envelope.action,
                `La acción '${envelope.action}' no está soportada por la extensión`,
                404,
                envelope.id
            );
        }
    }
}