using System.Runtime.InteropServices;
using Engine.Entities.ResponseDisplays;

namespace Engine.Presentation.Windows;

public class ResponseDisplayForm : Form
{
    private readonly ResponseDisplay _display;
    private readonly Label _lblResponse;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowDisplayAffinity(IntPtr hWnd, uint dwAffinity);

    private const uint WDA_EXCLUDEFROMCAPTURE = 0x00000011;

    private const int WS_EX_TOPMOST = 0x00000008;
    private const int WS_EX_TRANSPARENT = 0x00000020;
    private const int WS_EX_TOOLWINDOW = 0x00000080;
    private const int WS_EX_NOACTIVATE = 0x08000000;

    public ResponseDisplayForm(ResponseDisplay display)
    {
        _display = display ?? throw new ArgumentNullException(nameof(display));

        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.Manual;

        // Lee toda la configuración estética directamente de la entidad
        ApplyDomainStyles();

        _lblResponse = new Label
        {
            Text = _display.ResponseText,
            ForeColor = _display.TextColor,
            Font = _display.Font,
            AutoSize = false,
            Size = new Size(_display.Size.Width - 10, _display.Size.Height - 10),
            Location = new Point(5, 5)
        };

        Controls.Add(_lblResponse);
        SubscribeToDomainEvents();
    }

    private void ApplyDomainStyles()
    {
        Location = _display.Position;
        Size = _display.Size;
        BackColor = _display.BackColor;
        TransparencyKey = _display.BackColor;
        Opacity = _display.Opacity;
    }

    protected override CreateParams CreateParams
    {
        get
        {
            var cp = base.CreateParams;
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

    private void SubscribeToDomainEvents()
    {
        _display.OnTextChanged += (sender, text) => SafeInvoke(() => _lblResponse.Text = text);
        _display.OnPositionChanged += (sender, pos) => SafeInvoke(() => Location = pos);
        _display.OnVisibilityChanged += (sender, visible) => SafeInvoke(() => Visible = visible);
        _display.OnStyleChanged += (sender, e) => SafeInvoke(() =>
        {
            ApplyDomainStyles();
            _lblResponse.ForeColor = _display.TextColor;
            _lblResponse.Font = _display.Font;
        });
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
}