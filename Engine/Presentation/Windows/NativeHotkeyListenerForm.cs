using System.Runtime.InteropServices;
using Engine.Entities.Overlays;
using Engine.Entities.Selections;

namespace Engine.Presentation.Windows;

public class NativeHotkeyListener : Form
{
    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    private readonly Selection _selection;
    private readonly Overlay _overlay;

    private const int TRIGGER_HOTKEY_ID = 9000;
    private const int EXIT_HOTKEY_ID = 9001;
    private const int POSITION_HOTKEY_ID = 9003;

    private const int WM_HOTKEY = 0x0312;
    private const uint VK_F8 = 0x77;
    private const uint VK_F9 = 0x78;
    private const uint MOD_SHIFT = 0x0004;

    public NativeHotkeyListener(Selection selection, Overlay overlay)
    {
        _selection = selection ?? throw new ArgumentNullException(nameof(selection));
        _overlay = overlay ?? throw new ArgumentNullException(nameof(overlay));

        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        WindowState = FormWindowState.Minimized;
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        
        // F8 -> Iniciar Selección y Captura
        RegisterHotKey(Handle, TRIGGER_HOTKEY_ID, 0x0000, VK_F8);

        // F9 -> Mover Posición del Overlay al Cursor (Imperceptible en 0ms)
        RegisterHotKey(Handle, POSITION_HOTKEY_ID, 0x0000, VK_F9);

        // Shift + F8 -> Cierre Total del Programa
        RegisterHotKey(Handle, EXIT_HOTKEY_ID, MOD_SHIFT, VK_F8);
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WM_HOTKEY)
        {
            int hotkeyId = m.WParam.ToInt32();

            if (hotkeyId == TRIGGER_HOTKEY_ID)
            {
                _selection.RequestSelection();
            }
            else if (hotkeyId == POSITION_HOTKEY_ID)
            {
                // Posiciona el Overlay instantáneamente donde esté apuntando el ratón
                _overlay.SetPosition(Cursor.Position.X, Cursor.Position.Y);
            }
            else if (hotkeyId == EXIT_HOTKEY_ID)
            {
                Application.Exit();
            }
        }
        base.WndProc(ref m);
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        UnregisterHotKey(Handle, TRIGGER_HOTKEY_ID);
        UnregisterHotKey(Handle, POSITION_HOTKEY_ID);
        UnregisterHotKey(Handle, EXIT_HOTKEY_ID);
        base.OnFormClosing(e);
    }
}