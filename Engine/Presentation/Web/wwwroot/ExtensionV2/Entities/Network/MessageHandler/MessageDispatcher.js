import { ProviderEnvelope } from '../../IAProviders/DTO/ProviderEnvelope';
import { IClientMessageHandler } from './Interfaces/IClientMessageHandler';

/**
 * Despachador de mensajes del cliente. Se suscribe directamente a un cliente de red/puente
 * y delega la ejecución de comandos al manejador (handler) correspondiente.
 */
export class MessageDispatcher {
    /**
     * @param {Object} [client=null] Cliente WebSocket activo o puente de extensión (ExtensionClientBridge).
     * @param {IClientMessageHandler[]} [handlers=[]] Lista opcional de handlers iniciales.
     */
    constructor(client = null, handlers = []) {
        this.client = client;
        /** @type {Map<string, IClientMessageHandler>} */
        this.handlers = new Map();

        handlers.forEach(handler => this.registerHandler(handler));

        if (this.client) {
            this.attachClient(this.client);
        }
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
     * Suscribe el dispatcher al evento de recepción del cliente de red.
     * @param {Object} client
     */
    attachClient(client) {
        this.client = client;
        this.client.onMessage = async (envelope) => {
            await this.dispatch(envelope);
        };
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