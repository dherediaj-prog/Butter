import { IClientMessageHandler } from '../Interfaces/IClientMessageHandler.js';

export class CancelMessageHandler extends IClientMessageHandler {
    /**
     * @param {IIAProvider} provider Instancia del proveedor de IA inyectada al manejador.
     */
    constructor(provider) {
        super();
        this.provider = provider;
    }

    get action() {
        return "CANCEL";
    }

    async handle(client, envelope) {
        const cancelled = await this.provider.cancel();
        if (cancelled) {
            client.sendSuccess("CANCEL", { cancelled: true }, envelope.id);
        } else {
            client.sendFail("CANCEL", "No se encontró ningún proceso activo para detener", 404, envelope.id);
        }
    }
}