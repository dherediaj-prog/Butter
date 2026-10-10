export class PromptPayload {
    /**
     * @param {Object} [options]
     * @param {string|null} [options.prompt=null]
     * @param {string|null} [options.image=null]
     * @param {string} [options.mimeType="image/png"]
     */
    constructor({ prompt = null, image = null, mimeType = "image/png" } = {}) {
        this.prompt = prompt;
        this.image = image;
        this.mimeType = mimeType;
    }

    static from(data) {
        if (!data) return new PromptPayload();
        return new PromptPayload({
            prompt: data.prompt ?? data.Prompt ?? null,
            image: data.image ?? data.Image ?? null,
            mimeType: data.mimeType ?? data.MimeType ?? "image/png"
        });
    }
}