import { IIAProvider } from './Interfaces/IIAProvider.js';

/**
 * @implements {IIAProvider}
 */
export class GeminiProvider extends IIAProvider {
    constructor() {
        super();
        this.selectors = {
            inputBox: 'rich-textarea[atmentions], [atmentions]',
            editableArea: 'div[contenteditable="true"]',
            // Variantes de selectores para ubicar el botón de envío en el DOM dinámico de Gemini
            sendButtonCandidates: [
                'button[aria-label="Enviar mensaje"]',
                'button[aria-label*="Enviar"]',
                'button[aria-label*="Send"]',
                'gem-icon-button.send-button button',
                'gem-icon-button.submit button',
                'button:has(mat-icon[data-mat-icon-name="arrow_upward"])',
                'div[data-test-id="send-button-container"] button'
            ],
            stopIcon: 'mat-icon[data-mat-icon-name="stop"]',
            micIcon: 'mat-icon[data-mat-icon-name="mic"]',
            assistantResponse: 'message-content'
        };
    }

    /**
     * Escribe el prompt en el cuadro de texto editable.
     * @param {string} promptText
     */
    async writePrompt(promptText) {
        const area = document.querySelector(this.selectors.editableArea);
        if (!area) throw new Error("Área editable no encontrada en el DOM.");

        area.innerHTML = `<p>${promptText}</p>`;
        area.dispatchEvent(new Event('input', { bubbles: true }));
    }

    /**
     * Pega una imagen codificada en Base64/URL en el área editable simulando ClipboardEvent.
     * @param {string|File} source
     * @param {string} fileName
     */
    async insertImage(source, fileName = "capture.png") {
        const area = document.querySelector(this.selectors.editableArea);
        if (!area) throw new Error("Área editable no encontrada en el DOM.");

        let file = source;
        if (typeof source === 'string') {
            const res = await fetch(source);
            const blob = await res.blob();
            file = new File([blob], fileName, { type: "image/png" });
        }

        const dt = new DataTransfer();
        dt.items.add(file);
        area.dispatchEvent(new ClipboardEvent('paste', { clipboardData: dt, bubbles: true }));
    }

    /**
     * Localiza el botón de envío habilitado y ejecuta el clic, o emula la tecla Enter.
     */
    async send() {
        let btn = null;

        // Esperar hasta 3 segundos (15 reintentos) mientras Angular habilita el botón tras pegar imágenes/texto
        for (let i = 0; i < 15; i++) {
            for (const selector of this.selectors.sendButtonCandidates) {
                const candidate = document.querySelector(selector);
                if (candidate) {
                    const parentContainer = candidate.closest('gem-icon-button');
                    const isParentDisabled = parentContainer?.getAttribute('aria-disabled') === 'true';
                    const isBtnDisabled = candidate.disabled || candidate.getAttribute('aria-disabled') === 'true';

                    if (!isParentDisabled && !isBtnDisabled) {
                        btn = candidate;
                        break;
                    }
                }
            }

            if (btn) break;
            await new Promise(r => setTimeout(r, 200));
        }

        if (btn) {
            console.log("[GeminiProvider] 🚀 Botón de envío localizado. Ejecutando click()...");
            btn.click();
            return;
        }

        // Fallback: Si no se pudo hacer clic en el botón, emular la tecla Enter en el cuadro de texto
        console.warn("[GeminiProvider] ⚠️ Botón no clickeable directamente. Intentando envío mediante evento KeyboardEvent('Enter')...");
        const area = document.querySelector(this.selectors.editableArea);
        if (area) {
            area.dispatchEvent(new KeyboardEvent('keydown', {
                key: 'Enter',
                code: 'Enter',
                keyCode: 13,
                which: 13,
                bubbles: true
            }));
            return;
        }

        throw new Error("Botón de envío no encontrado ni habilitado en la interfaz de Gemini.");
    }

    /**
     * Cancela la generación activa si el botón de Stop está presente.
     * @returns {Promise<boolean>}
     */
    async cancel() {
        const stop = document.querySelector(this.selectors.stopIcon);
        if (stop) {
            const btn = stop.closest('button, [role="button"]');
            if (btn) {
                btn.click();
                return true;
            }
        }
        return false;
    }

    /**
     * Observa las mutaciones del DOM hasta que la IA termine de responder.
     * @param {number} initialCount Cantidad de respuestas existentes antes de enviar.
     * @returns {Promise<string>}
     */
    async observeResponse(initialCount = 0) {
        return new Promise((resolve) => {
            let started = false;
            const observer = new MutationObserver(() => {
                const isStop = !!document.querySelector(this.selectors.stopIcon);
                const isMic = !!document.querySelector(this.selectors.micIcon);
                const responses = document.querySelectorAll(this.selectors.assistantResponse);

                if (isStop || responses.length > initialCount) started = true;

                if (started && isMic && !isStop && responses.length > initialCount) {
                    const last = responses[responses.length - 1];
                    const text = last?.innerText?.trim() || '';
                    if (text) {
                        observer.disconnect();
                        resolve(text);
                    }
                }
            });

            observer.observe(document.body, {
                childList: true,
                subtree: true,
                characterData: true,
                attributes: true
            });
        });
    }
}