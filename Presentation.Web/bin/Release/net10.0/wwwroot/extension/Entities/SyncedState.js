class SyncedProperty extends EventTarget {
    #key;
    #value;

    constructor(storageKey, defaultValue) {
        super();
        this.#key = storageKey;
        this.#value = defaultValue;
        this.#bindCrossContextSync();
    }

    async init() {
        if (typeof chrome !== 'undefined' && chrome.storage?.local) {
            const res = await chrome.storage.local.get([this.#key]);
            if (res[this.#key] !== undefined) {
                this.#value = res[this.#key];
            }
        }
        return this.#value;
    }

    get value() {
        return this.#value;
    }

    set value(newValue) {
        if (this.#value !== newValue) {
            this.#value = newValue;
            this.#persist(newValue);
            this.#notify(newValue);
        }
    }

    onChange(callback) {
        this.addEventListener('change', (e) => callback(e.detail));
    }

    #persist(val) {
        if (typeof chrome !== 'undefined' && chrome.storage?.local) {
            chrome.storage.local.set({ [this.#key]: val });
        }
    }

    #notify(val) {
        this.dispatchEvent(new CustomEvent('change', { detail: val }));
    }

    #bindCrossContextSync() {
        if (typeof chrome !== 'undefined' && chrome.storage?.onChanged) {
            chrome.storage.onChanged.addListener((changes, area) => {
                if (area === 'local' && changes[this.#key]) {
                    const newValue = changes[this.#key].newValue;
                    if (this.#value !== newValue) {
                        this.#value = newValue;
                        this.#notify(this.#value);
                    }
                }
            });
        }
    }
}