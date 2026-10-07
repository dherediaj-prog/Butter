class ButterKnifeApp {
    #metadata;
    #configService;
    #providerService;
    #wsService;
    #reconnectInterval = 4000;

    constructor({metadata, configService, providerService, wsService}) {
        this.#metadata = metadata;
        this.#configService = configService;
        this.#providerService = providerService;
        this.#wsService = wsService;
    }

    async init() {
        await this.#configService.init();
        this.#configService.host.onChange(() => this.#connect());
        this.#bindExtensionMessages();
        await this.#connect();
    }

    #bindExtensionMessages() {
        if (typeof chrome !== 'undefined' && chrome.runtime?.onMessage) {
            chrome.runtime.onMessage.addListener((request, sender, sendResponse) => {
                if (request.action === 'GET_TAB_METADATA') {
                    sendResponse({
                        id: this.#metadata.id,
                        provider: this.#metadata.provider,
                        isConnected: this.#wsService.socket?.readyState === WebSocket.OPEN
                    });
                }
            });
        }
    }

    async #connect() {
        const socket = this.#wsService.connect(this.#configService.host.value);

        socket.addEventListener('open', () => {
            console.log(`[ButterKnife] Provider [${this.#metadata.provider} - ${this.#metadata.id}] listo.`);
        });

        socket.addEventListener('message', (event) => this.#handleIncomingMessage(event));
        socket.addEventListener('close', () => setTimeout(() => this.#connect(), this.#reconnectInterval));
        socket.addEventListener('error', (err) => console.error('[ButterKnife] Error de socket local:', err));
    }

    async #handleIncomingMessage(event) {
        try {
            const payload = JSON.parse(event.data);
            if (!payload.action) return;

            this.#wsService.sendResponse('', MessageStatus.LOADING);

            let resultText = '';
            if (payload.action === MessageAction.ASK && payload.prompt) {
                this.#wsService.sendResponse('', MessageStatus.PROCESSING);
                resultText = await this.#providerService.processPrompt(payload.prompt);
            } else if (payload.action === MessageAction.CAPTURED_IMAGE && payload.image) {
                this.#wsService.sendResponse('', MessageStatus.PROCESSING);
                const prompt = payload.prompt || "Analiza la imagen adjunta";
                resultText = await this.#providerService.processImagePrompt(payload.image, prompt);
            } else {
                return;
            }

            this.#wsService.sendResponse(resultText, MessageStatus.COMPLETED);
        } catch (err) {
            this.#wsService.sendError(err.message || String(err));
        }
    }
}

window.addEventListener('load', async () => {
    const metadata = new IAProviderMetadata();

    const app = new ButterKnifeApp({
        metadata,
        configService: new ConfigService(metadata.id),
        providerService: new IAProviderService(),
        wsService: new WebSocketService(metadata)
    });

    await app.init();
});