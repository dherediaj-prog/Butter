/**
 * Estados de procesamiento del proveedor.
 */
export const ProviderStatus = Object.freeze({
    Completed: "Completed",
    Processing: "Processing",
    Loading: "Loading",
    Failed: "Failed"
});

/**
 * Envoltorio estandarizado para la transmisión de mensajes vía WebSocket.
 */
export class ProviderEnvelope {
    /**
     * @param {Object} options
     * @param {string} options.action - Nombre de la acción o comando.
     * @param {any} [options.payload=null] - Datos adjuntos.
     * @param {string} [options.status=ProviderStatus.Completed] - Estado del procesamiento.
     * @param {string|null} [options.id=null] - ID único del paquete (se genera GUID de 32 hex si no se especifica).
     * @param {{ code: number, message: string }|null} [options.error=null] - Objeto de error si aplica.
     * @param {number|null} [options.timestamp=null] - Marca de tiempo Unix (ms).
     */
    constructor({
                    action,
                    payload = null,
                    status = ProviderStatus.Completed,
                    id = null,
                    error = null,
                    timestamp = null
                }) {
        this.action = action;
        this.payload = payload;
        this.status = status;
        this.id = id && id.trim() !== '' ? id : crypto.randomUUID().replace(/-/g, '');
        this.error = error;
        this.timestamp = timestamp ?? Date.now();
    }

    // --- Métodos de Fábrica Estáticos ---

    static success(action, payload, id = null) {
        return new ProviderEnvelope({
            action,
            payload,
            status: ProviderStatus.Completed,
            id
        });
    }

    static processing(action, payload = null, id = null) {
        return new ProviderEnvelope({
            action,
            payload,
            status: ProviderStatus.Processing,
            id
        });
    }

    static loading(action, payload = null, id = null) {
        return new ProviderEnvelope({
            action,
            payload,
            status: ProviderStatus.Loading,
            id
        });
    }

    static fail(action, errorMessage, errorCode = 500, id = null) {
        return new ProviderEnvelope({
            action,
            payload: null,
            status: ProviderStatus.Failed,
            id,
            error: { code: errorCode, message: errorMessage }
        });
    }

    /**
     * Deserializa un objeto JSON o texto en una instancia de ProviderEnvelope,
     * soportando tanto camelCase como PascalCase.
     */
    static from(data) {
        if (!data) return null;

        const obj = typeof data === 'string' ? JSON.parse(data) : data;

        return new ProviderEnvelope({
            action: obj.action ?? obj.Action,
            payload: obj.payload ?? obj.Payload ?? null,
            status: obj.status ?? obj.Status ?? ProviderStatus.Completed,
            id: obj.id ?? obj.Id ?? null,
            error: obj.error ?? obj.Error ?? null,
            timestamp: obj.timestamp ?? obj.Timestamp ?? null
        });
    }
}