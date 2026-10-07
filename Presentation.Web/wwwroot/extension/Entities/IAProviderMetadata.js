class IAProviderMetadata {
    #id;
    #provider;
    #url;

    constructor({ id = crypto.randomUUID(), url = window.location.href } = {}) {
        this.#id = id;
        this.#url = url;
        this.#provider = this.#detectProvider(this.#url);
        Object.freeze(this);
    }

    get id() { return this.#id; }
    get provider() { return this.#provider; }
    get url() { return this.#url; }

    #detectProvider(rawUrl) {
        try {
            const hostname = new URL(rawUrl).hostname.toLowerCase();
            if (hostname.includes('gemini')) return 'gemini';
            if (hostname.includes('chatgpt')) return 'chatgpt';
            if (hostname.includes('claude')) return 'claude';
            if (hostname.includes('deepseek')) return 'deepseek';
            return 'unknown';
        } catch {
            return 'unknown';
        }
    }

    toJSON() {
        return {
            id: this.#id,
            provider: this.#provider,
            url: this.#url
        };
    }
}