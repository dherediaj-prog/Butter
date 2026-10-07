class IAProviderService {
    constructor() {
        this.selectors = {
            inputBox: 'rich-textarea[atmentions], [atmentions]',
            sendButton: 'div[data-test-id="send-button-container"] button, div[data-test-id="send-button-container"] [role="button"]',
            assistantResponse: 'message-content[id^="message-content-id-r_"]',
            stopIcon: 'mat-icon[data-mat-icon-name="stop"]',
            micIcon: 'mat-icon[data-mat-icon-name="mic"]'
        };
        this.timings = {
            afterFillDelay: 400
        };
        this.isProcessing = false;
    }

    async processPrompt(promptText) {
        if (this.isProcessing) {
            throw new Error("El proveedor ya se encuentra procesando una solicitud.");
        }

        const inputBox = document.querySelector(this.selectors.inputBox);
        if (!inputBox) {
            throw new Error("No se encontró el elemento de entrada de texto en el DOM.");
        }

        try {
            this.isProcessing = true;

            const initialCount = document.querySelectorAll(this.selectors.assistantResponse).length;

            const targetDiv = inputBox.querySelector('div[contenteditable="true"]') || inputBox.querySelector('div') || inputBox;
            targetDiv.innerHTML = `<p>${promptText}</p>`;

            targetDiv.dispatchEvent(new Event('input', { bubbles: true }));
            inputBox.dispatchEvent(new Event('input', { bubbles: true }));

            await new Promise(r => setTimeout(r, this.timings.afterFillDelay));

            const sendBtn = document.querySelector(this.selectors.sendButton);
            if (!sendBtn) {
                throw new Error("No se encontró el botón de envío.");
            }

            sendBtn.click();

            return await this.observeResponse(initialCount);
        } finally {
            this.isProcessing = false;
        }
    }

    async processImagePrompt(base64DataUrl, promptText) {
        const res = await fetch(base64DataUrl);
        const blob = await res.blob();
        const file = new File([blob], "capture.png", { type: "image/png" });

        const dataTransfer = new DataTransfer();
        dataTransfer.items.add(file);

        const inputBox = document.querySelector(this.selectors.inputBox);
        if (!inputBox) {
            throw new Error("No se encontró el área de texto para adjuntar la imagen.");
        }

        const targetDiv = inputBox.querySelector('div[contenteditable="true"]') || inputBox.querySelector('div') || inputBox;

        const pasteEvent = new ClipboardEvent('paste', {
            clipboardData: dataTransfer,
            bubbles: true
        });
        targetDiv.dispatchEvent(pasteEvent);

        await new Promise(r => setTimeout(r, 800));
        return await this.processPrompt(promptText);
    }

    observeResponse(initialCount = 0) {
        return new Promise((resolve) => {
            let hasStartedStreaming = false;
            console.log(`[Butter Debug] 🔍 Observando estado de iconos (stop/mic). Conteo previo: ${initialCount}`);

            const observer = new MutationObserver(() => {
                const isStopVisible = !!document.querySelector(this.selectors.stopIcon);
                const isMicVisible = !!document.querySelector(this.selectors.micIcon);
                const responses = document.querySelectorAll(this.selectors.assistantResponse);

                // Detectar el inicio de la generación cuando aparece el icono Stop o un nuevo contenedor
                if (isStopVisible || responses.length > initialCount) {
                    hasStartedStreaming = true;
                }

                // Condición de finalización: El proceso inició, la respuesta existe, no hay botón Stop y volvió el Micrófono
                if (hasStartedStreaming && isMicVisible && !isStopVisible && responses.length > initialCount) {
                    const lastResponse = responses[responses.length - 1];
                    const text = lastResponse?.innerText?.trim() || '';

                    if (text.length > 0) {
                        console.log('[Butter Debug] ✅ Transición a icono Mic detectada. Respuesta completa.');
                        observer.disconnect();
                        resolve(text);
                    }
                }
            });

            // Se habilita attributes: true para detectar el cambio del atributo data-mat-icon-name
            observer.observe(document.body, {
                childList: true,
                subtree: true,
                characterData: true,
                attributes: true
            });
        });
    }
}