class DashboardPresentation {
    #elements;

    constructor() {
        this.#elements = {
            status: document.getElementById('status'),
            logs: document.getElementById('logs'),
            img: document.getElementById('captured-image'),
            previewContainer: document.getElementById('preview-container'),
            noCaptureText: document.getElementById('no-capture-text'),
            aiText: document.getElementById('aiText'),
            promptText: document.getElementById('promptText')
        };

        // Pilar 2: Autoprotección (resguardo interno de integridad del DOM)
        for (const [key, el] of Object.entries(this.#elements)) {
            if (!el) throw new Error(`Elemento de DOM faltante para: ${key}`);
        }
    }

    renderStatus(isConnected) {
        this.#elements.status.innerText = isConnected ? 'Conectado' : 'Desconectado';
        this.#elements.status.className = `status ${isConnected ? 'status-on' : 'status-off'}`;
    }

    log(msg) {
        if (!msg) return;
        this.#elements.logs.innerHTML += `[${new Date().toLocaleTimeString()}] ${msg}<br>`;
        this.#elements.logs.scrollTop = this.#elements.logs.scrollHeight;
    }

    renderCapturedImage(base64Src) {
        if (!base64Src) throw new Error("La fuente de la imagen es requerida.");
        this.#elements.img.src = base64Src;
        this.#elements.previewContainer.style.display = 'block';
        this.#elements.noCaptureText.style.display = 'none';
    }

    get aiResponseText() {
        return this.#elements.aiText.value.trim();
    }

    get promptText() {
        return this.#elements.promptText.value.trim();
    }

    clearPromptInput() {
        this.#elements.promptText.value = '';
    }

    bindAction(buttonId, handler) {
        if (!buttonId || typeof handler !== 'function') {
            throw new Error("buttonId y handler son requeridos.");
        }
        const btn = document.getElementById(buttonId);
        if (!btn) throw new Error(`Botón no encontrado en el DOM: ${buttonId}`);
        btn.addEventListener('click', handler);
    }
}

class DashboardEngine {
    #socket = null;

    connectWebSocket() {
        if (this.#socket) {
            this.#socket.close();
        }

        const protocol = window.location.protocol === 'https:' ? 'wss:' : 'ws:';
        const wsUrl = `${protocol}//${window.location.host}/ws/chat`;

        this.#socket = new WebSocket(wsUrl);
        return this.#socket;
    }

    sendWsPayload(payload) {
        // Pilar 2: Autoprotección (Valida su propio estado interno y parámetros)
        if (!payload) throw new Error("El payload es requerido.");
        if (!this.#socket || this.#socket.readyState !== WebSocket.OPEN) {
            throw new Error("El WebSocket no está conectado.");
        }
        this.#socket.send(JSON.stringify(payload));
    }
}

class DashboardApp {
    #presentation;
    #engine;

    // Pilar 1 & 3: Inyección Externa en Plano 1D
    constructor({ presentation, engine }) {
        if (!presentation || !engine) {
            throw new Error("DashboardApp requiere presentation y engine en el constructor.");
        }
        this.#presentation = presentation;
        this.#engine = engine;
    }

    init() {
        this.#bindEvents();
        this.#startConnection();
    }

    // Pilar 4: Orquestación superior de eventos entre componentes desacoplados
    #bindEvents() {
        this.#presentation.bindAction('btn-reconnect', () => this.#startConnection());
        this.#presentation.bindAction('btn-send-response', () => this.#handleSendResponse());
        this.#presentation.bindAction('btn-send-prompt', () => this.#handleSendPrompt());
        this.#presentation.bindAction('btn-download-ext', () => this.#handleDownloadExtension());
    }

    #startConnection() {
        const socket = this.#engine.connectWebSocket();

        socket.addEventListener('open', () => {
            this.#presentation.renderStatus(true);
            this.#presentation.log(`Conectado exitosamente a ${socket.url}`);
        });

        socket.addEventListener('message', (event) => this.#handleIncomingMessage(event));

        socket.addEventListener('close', () => {
            this.#presentation.renderStatus(false);
            this.#presentation.log('WebSocket Desconectado.');
        });

        socket.addEventListener('error', () => {
            this.#presentation.log('Error de conexión en WebSocket.');
        });
    }

    #handleIncomingMessage(event) {
        try {
            const data = JSON.parse(event.data);
            if (data.action === MessageAction.CAPTURED_IMAGE && data.image) {
                this.#presentation.renderCapturedImage(data.image);
                this.#presentation.log('Imagen de captura recibida y visualizada.');
                return;
            }
            this.#presentation.log(`Mensaje recibido por WS: ${event.data}`);
        } catch {
            this.#presentation.log(`Mensaje plano recibido por WS: ${event.data}`);
        }
    }

    #handleSendResponse() {
        try {
            const text = this.#presentation.aiResponseText;
            // Pilar 2: Confianza ciega en la API de engine.sendWsPayload
            this.#engine.sendWsPayload({ action: MessageAction.RESPONSE, text });
            this.#presentation.log(`Enviada respuesta por WS: ${text}`);
        } catch (err) {
            alert(err.message);
        }
    }

    #handleSendPrompt() {
        try {
            const prompt = this.#presentation.promptText;
            if (!prompt) return;

            // Pilar 5 y 6: 100% WebSocket directo, sin fetch ni wrappers HTTP
            this.#engine.sendWsPayload({ action: MessageAction.ASK, prompt });
            this.#presentation.log(`Prompt enviado por WebSocket: ${prompt}`);
            this.#presentation.clearPromptInput();
        } catch (e) {
            this.#presentation.log(`Error al enviar por WS: ${e.message}`);
        }
    }

    #handleDownloadExtension() {
        this.#presentation.log('Iniciando descarga de ButterKnife-Extension.zip...');
        window.location.href = '/api/extension/download';
    }
}

window.addEventListener('load', () => {
    // Pilar 3: Inyección de dependencias en Plano 1D desde la raíz
    const presentation = new DashboardPresentation();
    const engine = new DashboardEngine();

    const app = new DashboardApp({ presentation, engine });
    app.init();
});