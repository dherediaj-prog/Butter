export class ResponsePayload {
    /**
     * @param {Object} [options]
     * @param {string} [options.text=""] - Texto generado por la IA.
     * @param {string|null} [options.providerName=null] - Nombre opcional del proveedor (ej: "Gemini", "ChatGPT").
     */
    constructor({text = "", providerName = null} = {}) {
        this.text = text;
        this.providerName = providerName;
    }

    /**
     * Instancia el DTO desde datos desestructurados (compatible con PascalCase y camelCase).
     * @param {Object} data
     * @returns {ResponsePayload}
     */
    static from(data) {
        if (!data) return new ResponsePayload();
        return new ResponsePayload({
            text: data.text ?? data.Text ?? "",
            providerName: data.providerName ?? data.ProviderName ?? null
        });
    }
}