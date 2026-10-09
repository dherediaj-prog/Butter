export class IPService {
    /**
     * Descubre la IP local de la interfaz de red usando WebRTC.
     * Funciona en Service Workers y Content Scripts.
     * @returns {Promise<string>}
     */
    static async getLocalIPAddress() {
        return new Promise((resolve) => {
            // Timeout de seguridad en caso de que el navegador bloquee WebRTC o use mDNS
            const timer = setTimeout(() => resolve('127.0.0.1'), 1500);

            try {
                const pc = new RTCPeerConnection({iceServers: []});
                pc.createDataChannel('');
                pc.createOffer().then((offer) => pc.setLocalDescription(offer));

                pc.onicecandidate = (ice) => {
                    if (!ice || !ice.candidate || !ice.candidate.candidate) return;

                    // Regex para capturar direcciones IPv4 (ej. 192.168.x.x, 10.x.x.x)
                    const ipMatch = /([0-9]{1,3}(\.[0-9]{1,3}){3})/.exec(ice.candidate.candidate);

                    if (ipMatch && !ipMatch[1].startsWith('127.')) {
                        clearTimeout(timer);
                        pc.close();
                        resolve(ipMatch[1]);
                    }
                };
            } catch (error) {
                clearTimeout(timer);
                resolve('127.0.0.1');
            }
        });
    }
}