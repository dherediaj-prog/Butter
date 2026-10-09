import {IIAProvider} from './Interfaces/IIAProvider.js';

/**
 * @implements {IIAProvider}
 */
export class GeminiProvider extends IIAProvider {
    constructor() {
        super();
        this.selectors = {
            inputBox: 'rich-textarea[atmentions], [atmentions]',
            editableArea: 'div[contenteditable="true"]',
            sendButton: 'div[data-test-id="send-button-container"] button',
            stopIcon: 'mat-icon[data-mat-icon-name="stop"]',
            micIcon: 'mat-icon[data-mat-icon-name="mic"]',
            assistantResponse: 'message-content[id^="message-content-id-r_"]'
        };
    }

    async writePrompt(promptText) {
        const area = document.querySelector(this.selectors.editableArea);
        if (!area) throw new Error("Área editable no encontrada.");

        area.innerHTML = `<p>${promptText}</p>`;
        area.dispatchEvent(new Event('input', {bubbles: true}));
    }

    async insertImage(source, fileName = "capture.png") {
        const area = document.querySelector(this.selectors.editableArea);
        if (!area) throw new Error("Área editable no encontrada.");

        let file = source;
        if (typeof source === 'string') {
            const res = await fetch(source);
            const blob = await res.blob();
            file = new File([blob], fileName, {type: "image/png"});
        }

        const dt = new DataTransfer();
        dt.items.add(file);
        area.dispatchEvent(new ClipboardEvent('paste', {clipboardData: dt, bubbles: true}));
    }

    async send() {
        const btn = document.querySelector(this.selectors.sendButton);
        if (!btn) throw new Error("Botón de envío no encontrado.");
        btn.click();
    }

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