# Butter

## Estructura

- `Engine`: biblioteca que contiene la lógica, la presentación Windows Forms y la presentación web.
- `App`: punto de entrada ejecutable. `Configuration` registra las dependencias y `Bootstrapping` configura el servidor web y conecta los componentes de `Engine`.

## Ejecutar en JetBrains Rider

1. Instala el SDK de .NET 10 y abre `Butter.sln` en Rider.
2. Selecciona `App` como proyecto de inicio y pulsa **Run**.
3. Sigue las instrucciones de conexión que aparecen en el overlay. El panel de control abre en `http://<IP-local>:5000`; en el mismo equipo también sirve <http://localhost:5000>. La aplicación queda en segundo plano, sin ventana ni icono en la barra de tareas: **F8** inicia una captura, **F9** mueve el overlay y **Shift+F8** cierra la aplicación.

También puedes iniciarla desde una terminal en la raíz del repositorio con `dotnet run --project App\App.csproj`.

El servidor escucha en el puerto 5000 en las interfaces de red disponibles para que la extensión del navegador pueda conectarse. Si Windows Firewall lo bloquea, permite el acceso a ButterKnife o configura la regla de firewall correspondiente.
