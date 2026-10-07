using System.Runtime.InteropServices;
using Engine.Entities.Overlays;

namespace Engine.Presentation.Windows;

public class PassiveOverlayForm : Form
{
    private readonly Overlay _overlay;
    private readonly Label _lblResponse;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowDisplayAffinity(IntPtr hWnd, uint dwAffinity);

    private const uint WDA_EXCLUDEFROMCAPTURE = 0x00000011;

    private const int WS_EX_TOPMOST = 0x00000008;
    private const int WS_EX_TRANSPARENT = 0x00000020;
    private const int WS_EX_TOOLWINDOW = 0x00000080;
    private const int WS_EX_NOACTIVATE = 0x08000000;

    public PassiveOverlayForm(Overlay overlayRef)
    {
        _overlay = overlayRef ?? throw new ArgumentNullException(nameof(overlayRef));

        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.Manual;
        Location = _overlay.Position;
        
        // Contenedor grande de tamaño fijo para albergar bloques extensos de texto sin cortarse
        Size = new Size(850, 650);

        BackColor = Color.Black;
        TransparencyKey = Color.Black;
        
        // Opacidad equilibrada para mantener la discreción sin perder visibilidad
        Opacity = 0.80;

        _lblResponse = new Label
        {
            Text = _overlay.ResponseText,
            // Tono gris claro legible sobre cualquier fondo
            ForeColor = Color.FromArgb(210, 215, 220),
            Font = new Font("Consolas", 10f, FontStyle.Regular),
            AutoSize = false,
            // Tamaño amplio dentro de la ventana con margen de 5px
            Size = new Size(840, 640),
            Location = new Point(5, 5)
        };

        Controls.Add(_lblResponse);
        SubscribeToEngineEvents();
    }

    protected override CreateParams CreateParams
    {
        get
        {
            CreateParams cp = base.CreateParams;
            cp.ExStyle |= WS_EX_TOPMOST | WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE;
            return cp;
        }
    }

    protected override bool ShowWithoutActivation => true;

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        SetWindowDisplayAffinity(Handle, WDA_EXCLUDEFROMCAPTURE);
    }

    private void SubscribeToEngineEvents()
    {
        _overlay.OnTextChanged += (sender, text) => SafeInvoke(() => _lblResponse.Text = text);
        _overlay.OnPositionChanged += (sender, pos) => SafeInvoke(() => Location = pos);
        _overlay.OnVisibilityChanged += (sender, visible) => SafeInvoke(() => Visible = visible);
    }

    private void SafeInvoke(Action action)
    {
        if (IsDisposed || !IsHandleCreated) return;

        if (InvokeRequired)
        {
            Invoke(action);
            return;
        }

        action();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _overlay.OnTextChanged -= (sender, text) => { };
        _overlay.OnPositionChanged -= (sender, pos) => { };
        _overlay.OnVisibilityChanged -= (sender, visible) => { };
        base.OnFormClosing(e);
    }
}