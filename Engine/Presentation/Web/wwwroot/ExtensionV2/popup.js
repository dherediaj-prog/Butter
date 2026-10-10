document.addEventListener('DOMContentLoaded', () => {
    const statusDot = document.getElementById('statusDot');
    const statusText = document.getElementById('statusText');
    const serverUrl = document.getElementById('serverUrl');

    const inputIp = document.getElementById('inputIp');
    const inputPort = document.getElementById('inputPort');
    const btnConnect = document.getElementById('btnConnect');
    const btnReconnect = document.getElementById('btnReconnect');

    // Cargar la configuración guardada al abrir el popup
    chrome.storage.local.get(['serverIp', 'serverPort'], (result) => {
        if (result.serverIp) {
            inputIp.value = result.serverIp;
        }
        if (result.serverPort) {
            inputPort.value = result.serverPort;
        }
    });

    function checkStatus() {
        chrome.runtime.sendMessage({ type: "GET_STATUS" }, (response) => {
            if (response?.isConnected) {
                statusDot.classList.add('connected');
                statusText.innerText = "Conectado a WinForms";
            } else {
                statusDot.classList.remove('connected');
                statusText.innerText = "Desconectado";
            }
            serverUrl.innerText = response?.url || "Esperando conexión...";
        });
    }

    // Botón para actualizar IP/Puerto y conectar
    btnConnect.addEventListener('click', () => {
        const ip = inputIp.value.trim();
        const port = inputPort.value.trim();

        if (!ip || !port) {
            alert("Por favor, ingresa una IP y un puerto válidos.");
            return;
        }

        btnConnect.innerText = "Conectando...";
        btnConnect.disabled = true;

        chrome.runtime.sendMessage({
            type: "UPDATE_CONNECTION",
            ip: ip,
            port: port
        }, (response) => {
            btnConnect.innerText = "Conectar";
            btnConnect.disabled = false;

            if (response?.success) {
                checkStatus(); // Refrescar UI si fue exitoso
            } else {
                alert("Error al intentar actualizar la conexión.");
            }
        });
    });

    // Botón para reiniciar por completo la extensión en caso de fallo crítico
    btnReconnect.addEventListener('click', () => {
        chrome.runtime.reload();
    });

    // Verificar el estado actual al abrir el popup
    checkStatus();
});