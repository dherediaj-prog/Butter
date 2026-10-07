class WebSocketService {
    #metadata;
    #socket = null;

    constructor(metadata) {
        if (!metadata) throw new Error("IAProviderMetadata es requerido.");
        this.#metadata = metadata;
    }

    get socket() {
        return this.#socket;
    }

    connect(host) {
        if (!host) throw new Error("El host es requerido para conectar.");

        if (this.#socket) {
            this.#socket.close();
        }

        const rawHost = host.trim().replace(/^wss?:\/\//i, '');
        const wsUrl = `ws://${rawHost}/ws/chat`;
        console.log(`[ButterKnife] Conectando a ${wsUrl}...`);

        this.#socket = new WebSocket(wsUrl);

        this.#socket.addEventListener('open', () => {
            this.sendPayload({
                action: MessageAction.REGISTER,
                ...this.#metadata.toJSON()
            });
        });

        return this.#socket;
    }

    sendResponse(text, status = MessageStatus.COMPLETED) {
        if (!Object.values(MessageStatus).includes(status)) {
            throw new Error(`Estado inválido: ${status}`);
        }

        this.sendPayload({
            action: MessageAction.RESPONSE,
            status,
            ...this.#metadata.toJSON(),
            text
        });
    }

    sendError(errorMessage) {
        this.sendPayload({
            action: MessageAction.ERROR,
            status: MessageStatus.FAILED,
            ...this.#metadata.toJSON(),
            error: errorMessage
        });
    }

    sendPayload(payload) {
        if (!this.#socket) throw new Error("El WebSocket no ha sido inicializado.");
        this.#socket.send(JSON.stringify(payload));
    }
}