import {ProviderEnvelope} from '../../../IAProviders/DTO/ProviderEnvelope.js';
import {WebSocketClient} from '../../WebSocketClients/WebSocketClient.js';

/**
 * Contrato base para los manejadores de mensajes del lado del cliente.
 */
export class IClientMessageHandler {
    /**
     * Nombre de la acción que procesa este manejador (ej: "ASK", "CANCEL", "WRITE_PROMPT").
     * @type {string}
     */
    get action() {
        throw new Error(`[${this.constructor.name}] La propiedad 'action' debe ser implementada.`);
    }

    /**
     * Ejecuta la lógica del manejador.
     * @param {WebSocketClient} client Instancia del cliente de red o puente.
     * @param {ProviderEnvelope} envelope Paquete entrante desde C#.
     * @returns {Promise<void>}
     */
    async handle(client, envelope) {
        throw new Error(`[${this.constructor.name}] El método 'handle()' debe ser implementado.`);
    }
}