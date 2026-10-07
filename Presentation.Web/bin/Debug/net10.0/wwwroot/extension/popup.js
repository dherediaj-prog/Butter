class PopupPresentation {
    #hostInput;
    #saveBtn;
    #statusEl;

    constructor() {
        this.#hostInput = document.getElementById('hostInput');
        this.#saveBtn = document.getElementById('saveBtn');
        this.#statusEl = document.getElementById('status');

        if (!this.#hostInput || !this.#saveBtn || !this.#statusEl) {
            throw new Error("Elementos requeridos del DOM no fueron encontrados en popup.html.");
        }
    }

    get hostValue() {
        return this.#hostInput.value;
    }

    set hostValue(val) {
        this.#hostInput.value = val || '';
    }

    renderStatus(isConnected) {
        this.#statusEl.innerText = isConnected ? 'Conectado' : 'Desconectado';
        this.#statusEl.className = `status ${isConnected ? 'status-on' : 'status-off'}`;
    }

    onSave(handler) {
        this.#saveBtn.addEventListener('click', handler);
    }
}

class PopupApp {
    #presentation;
    #configService = null;

    constructor({presentation}) {
        if (!presentation) throw new Error("PopupPresentation es requerido.");
        this.#presentation = presentation;
    }

    async init() {
        this.#presentation.onSave(() => this.#handleSave());

        const tabId = await this.#getActiveTabId();
        if (!tabId) {
            this.#presentation.renderStatus(false);
            return;
        }

        const tabMetadata = await this.#getTabMetadata(tabId);
        if (!tabMetadata?.id) {
            this.#presentation.renderStatus(false);
            return;
        }

        // Instanciamos ConfigService aislado con la UUID de la pestaña activa
        this.#configService = new ConfigService(tabMetadata.id);
        await this.#configService.init();

        this.#presentation.hostValue = this.#configService.host.value;
        this.#presentation.renderStatus(tabMetadata.isConnected ?? false);
    }

    #handleSave() {
        if (!this.#configService) return;

        // ConfigService se autoprotege y limpia el host recibido
        this.#configService.setHost(this.#presentation.hostValue);
        window.close();
    }

    async #getActiveTabId() {
        const [tab] = await chrome.tabs.query({active: true, currentWindow: true});
        return tab?.id;
    }

    async #getTabMetadata(tabId) {
        try {
            return await chrome.tabs.sendMessage(tabId, {action: 'GET_TAB_METADATA'});
        } catch {
            return null;
        }
    }
}

document.addEventListener('DOMContentLoaded', async () => {
    const app = new PopupApp({
        presentation: new PopupPresentation()
    });
    await app.init();
});