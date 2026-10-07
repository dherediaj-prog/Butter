class ConfigService {
    static DEFAULT_HOST = '192.168.0.50:5000';
    #scopeKey;

    constructor(scopeKey) {
        if (!scopeKey) throw new Error("El scopeKey único es requerido para ConfigService.");
        this.#scopeKey = scopeKey.trim().toLowerCase();

        this.host = new SyncedProperty(`butterHost_${this.#scopeKey}`, ConfigService.DEFAULT_HOST);
    }

    async init() {
        await this.host.init();
        return this;
    }

    setHost(rawHost) {
        if (!rawHost) {
            this.host.value = ConfigService.DEFAULT_HOST;
            return;
        }
        this.host.value = rawHost.trim().replace(/^ws:\/\//i, '').replace(/\/.*$/, '');
    }
}