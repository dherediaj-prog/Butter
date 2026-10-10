/**
 * Contrato base para proveedores de IA en la web/extensión.
 * Define la API granular obligatoria que cada proveedor (Gemini, ChatGPT, Claude, etc.) debe implementar.
 *
 * @interface IIAProvider
 */
export class IIAProvider {
    /**
     * Escribe el texto en la caja de entrada sin enviarlo.
     * @param {string} promptText - Texto a escribir.
     * @returns {Promise<void>}
     */
    async writePrompt(promptText) {
        throw new Error(`[${this.constructor.name}] El método 'writePrompt()' debe ser implementado.`);
    }

    /**
     * Adjunta o inserta una imagen en el área de entrada.
     * @param {string | Blob | File} source - URL Base64, Blob o File de la imagen.
     * @param {string} [fileName="capture.png"] - Nombre opcional del archivo.
     * @returns {Promise<void>}
     */
    async insertImage(source, fileName = "capture.png") {
        throw new Error(`[${this.constructor.name}] El método 'insertImage()' debe ser implementado.`);
    }

    /**
     * Hace clic en el botón de envío para procesar el prompt.
     * @returns {Promise<void>}
     */
    async send() {
        throw new Error(`[${this.constructor.name}] El método 'send()' debe ser implementado.`);
    }

    /**
     * Cancela la respuesta en progreso haciendo clic en el botón de parada/stop.
     * @returns {Promise<boolean>} Devuelve true si se logró detener la generación.
     */
    async cancel() {
        throw new Error(`[${this.constructor.name}] El método 'cancel()' debe ser implementado.`);
    }

    /**
     * Escucha los cambios del DOM hasta que la respuesta de la IA termine de transmitirse.
     * @param {number} [initialCount=0] Conteo previo de respuestas en el DOM.
     * @returns {Promise<string>} Texto completo de la respuesta final.
     */
    async observeResponse(initialCount = 0) {
        throw new Error(`[${this.constructor.name}] El método 'observeResponse()' debe ser implementado.`);
    }
}