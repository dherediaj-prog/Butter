import { IClientMessageHandler } from '../Interfaces/IClientMessageHandler.js';
import { PromptPayload } from '../Payloads/PromptPayload.js';
import { ResponsePayload } from '../Payloads/ResponsePayload.js';

export class PromptMessageHandler extends IClientMessageHandler {
    /**
     * @param {IIAProvider} provider Instancia del proveedor de IA inyectada al manejador.
     */
    constructor(provider) {
        super();
        this.provider = provider;
        console.log(`[PromptMessageHandler] 🛠️ Handler instanciado con proveedor: ${provider?.constructor?.name ?? 'Desconocido'}`);
    }

    get action() {
        return "ASK";
    }

    async handle(client, envelope) {
        console.log(`[PromptMessageHandler] 📩 Evento 'ASK' recibido [ID: ${envelope?.id}]:`, envelope);

        const payload = PromptPayload.from(envelope.payload);
        console.log("[PromptMessageHandler] 📦 Payload deserializado:", payload);

        if (!payload.prompt && !payload.image) {
            console.warn("[PromptMessageHandler] ⚠️ Payload rechazado: falta texto e imagen.");
            client.sendFail("AI_RESPONSE", "Se requiere al menos un texto o una imagen para procesar", 400, envelope.id);
            return;
        }

        try {
            // 1. Notificar a C# que comenzó el procesamiento
            console.log("[PromptMessageHandler] ⏳ Notificando estado 'Processing' a C#...");
            client.sendProcessing("AI_RESPONSE", null, envelope.id);

            // 2. Insertar imagen si está presente
            if (payload.image) {
                console.log("[PromptMessageHandler] 🖼️ Adjuntando imagen a la interfaz...");
                const imageSource = payload.image.startsWith('data:')
                    ? payload.image
                    : `data:${payload.mimeType};base64,${payload.image}`;

                await this.provider.insertImage(imageSource);
                console.log("[PromptMessageHandler] ✅ Imagen adjuntada con éxito.");
            }

            // 3. Escribir prompt
            if (payload.prompt) {
                console.log(`[PromptMessageHandler] ✏️ Escribiendo texto (${payload.prompt.length} chars): "${payload.prompt.substring(0, 30)}..."`);
                await this.provider.writePrompt(payload.prompt);
                console.log("[PromptMessageHandler] ✅ Texto escrito en el input del DOM.");
            }

            // 4. Enviar a la IA en el DOM
            console.log("[PromptMessageHandler] 🚀 Ejecutando clic de envío en la IA...");
            await this.provider.send();
            console.log("[PromptMessageHandler] ✅ Clic de envío realizado.");

            // 5. Escuchar/observar la respuesta generada
            console.log("[PromptMessageHandler] 👀 Iniciando MutationObserver para esperar la respuesta de la IA...");
            const responseText = await this.provider.observeResponse();
            console.log(`[PromptMessageHandler] 🎉 Respuesta capturada de la IA (${responseText.length} chars):`, responseText);

            // 6. Enviar la respuesta encapsulada en el ResponsePayload hacia C#
            const responsePayload = new ResponsePayload({
                text: responseText,
                providerName: this.provider.constructor.name
            });

            console.log("[PromptMessageHandler] 📤 Enviando 'Success' a C#:", responsePayload);
            client.sendSuccess("AI_RESPONSE", responsePayload, envelope.id);
        } catch (error) {
            console.error("[PromptMessageHandler] ❌ Error crítico procesando la consulta:", error);
            client.sendFail("AI_RESPONSE", error.message, 500, envelope.id);
        }
    }
}