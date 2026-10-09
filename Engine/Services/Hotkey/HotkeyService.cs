using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using Engine.Services.Hotkey.Enums;

namespace Engine.Services.Hotkey;

public sealed class HotkeyService : IDisposable
{
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    private readonly MessageWindow _window;
    private readonly ConcurrentDictionary<int, Action> _callbacks = new();
    private int _currentHotkeyId = 9000; // ID inicial arbitrario

    public HotkeyService()
    {
        // Se inicializa la ventana invisible interceptora de mensajes
        _window = new MessageWindow(this);
    }

    public int Register(HotkeyModifiers modifiers, Keys key, Action onExecute)
    {
        ArgumentNullException.ThrowIfNull(onExecute);

        int id = Interlocked.Increment(ref _currentHotkeyId);
        uint vk = (uint)key;

        if (RegisterHotKey(_window.Handle, id, (uint)modifiers, vk))
        {
            _callbacks[id] = onExecute;
            return id;
        }

        throw new InvalidOperationException(
            $"No se pudo registrar el atajo: {modifiers} + {key}. Puede estar en uso por otra aplicación.");
    }

    public bool Unregister(int hotkeyId)
    {
        if (_callbacks.TryRemove(hotkeyId, out _))
        {
            return UnregisterHotKey(_window.Handle, hotkeyId);
        }

        return false;
    }

    public void Dispose()
    {
        // Limpiamos todos los hotkeys registrados al cerrar la app
        foreach (var id in _callbacks.Keys)
        {
            UnregisterHotKey(_window.Handle, id);
        }

        _callbacks.Clear();
        _window.DestroyHandle();
    }

    /// <summary>
    /// Ventana nativa imperceptible (Message-Only Window).
    /// Su único propósito es interceptar WM_HOTKEY del SO.
    /// </summary>
    private sealed class MessageWindow : NativeWindow
    {
        private const int WM_HOTKEY = 0x0312;
        private readonly HotkeyService _service;

        public MessageWindow(HotkeyService service)
        {
            _service = service;
            // Al crear los CreateParams vacíos, se crea un handle a nivel de sistema sin UI
            CreateHandle(new CreateParams());
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_HOTKEY)
            {
                int id = m.WParam.ToInt32();
                if (_service._callbacks.TryGetValue(id, out var action))
                {
                    // Ejecuta la acción vinculada de forma segura
                    action.Invoke();
                }
            }

            base.WndProc(ref m);
        }
    }
}