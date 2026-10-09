document.addEventListener('DOMContentLoaded', () => {
    const statusDot = document.getElementById('statusDot');
    const statusText = document.getElementById('statusText');
    const serverUrl = document.getElementById('serverUrl');
    const btnReconnect = document.getElementById('btnReconnect');

    function checkStatus() {
        chrome.runtime.sendMessage({ type: "GET_STATUS" }, (response) => {
            if (response?.isConnected) {
                statusDot.classList.add('connected');
                statusText.innerText = "Conectado a WinForms";
            } else {
                statusDot.classList.remove('connected');
                statusText.innerText = "Desconectado";
            }
            serverUrl.innerText = response?.url || "Buscando IP local...";
        });
    }

    btnReconnect.addEventListener('click', () => {
        chrome.runtime.reload();
    });

    checkStatus();
});