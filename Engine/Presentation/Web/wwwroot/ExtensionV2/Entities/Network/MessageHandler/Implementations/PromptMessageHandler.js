import { IClientMessageHandler } from '../Interfaces/IClientMessageHandler';
import { PromptPayload } from '../Payloads/PromptPayload';
import { ResponsePayload } from '../Payloads/ResponsePayload';

export class PromptMessageHandler extends IClientMessageHandler {
    /**
     * @param {IIAProvider} provider Instancia del proveedor de IA inyectada al manejador.
     */
    constructor(provider) {
        super();
        this.provider = provider;
    }

    get action() {
        return "ASK";
    }

    async handle(client, envelope) {
        const payload = PromptPayload.from(envelope.payload);

        if (!payload.prompt && !payload.image) {
            client.sendFail("AI_RESPONSE", "Se requiere al menos un texto o una imagen para procesar", 400, envelope.id);
            return;
        }

        try {
            // 1. Notificar a C# que comenzó el procesamiento
            client.sendProcessing("AI_RESPONSE", null, envelope.id);

            // 2. Insertar imagen si está presente
            if (payload.image) {
                const imageSource = payload.image.startsWith('data:')
                    ? payload.image
                    : `data:${payload.mimeType};base64,${payload.image}`;

                await this.provider.insertImage(imageSource);
            }

            // 3. Escribir prompt
            if (payload.prompt) {
                await this.provider.writePrompt(payload.prompt);
            }

            // 4. Enviar a la IA en el DOM
            await this.provider.send();

            // 5. Escuchar/observar la respuesta generada
            const responseText = await this.provider.observeResponse();

            // 6. Enviar la respuesta encapsulada en el ResponsePayload hacia C#
            const responsePayload = new ResponsePayload({
                text: responseText,
                providerName: this.provider.constructor.name
            });

            client.sendSuccess("AI_RESPONSE", responsePayload, envelope.id);
        } catch (error) {
            client.sendFail("AI_RESPONSE", error.message, 500, envelope.id);
        }
    }
}