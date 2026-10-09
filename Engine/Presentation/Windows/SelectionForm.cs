using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using Engine.Entities.Selections;

namespace Engine.Presentation.Windows;

public class SelectionForm : Form
{
    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    private readonly Selection _selection;

    private const int ESC_HOTKEY_ID = 9002;
    private const int WM_HOTKEY = 0x0312;
    private const uint VK_ESCAPE = 0x1B;

    private const int WM_MOUSEACTIVATE = 0x0021;
    private const int MA_NOACTIVATE = 3;

    private const int WS_EX_TOPMOST = 0x00000008;
    private const int WS_EX_TOOLWINDOW = 0x00000080;
    private const int WS_EX_NOACTIVATE = 0x08000000;

    public SelectionForm(Selection selection)
    {
        _selection = selection ?? throw new ArgumentNullException(nameof(selection));

        FormBorderStyle = FormBorderStyle.None;
        WindowState = FormWindowState.Maximized;
        TopMost = true;
        Cursor = Cursors.Cross;
        DoubleBuffered = true;

        // Capa nativa casi invisible que captura eventos del ratón a nivel de DWM
        BackColor = Color.Black;
        Opacity = 0.05;

        _selection.Changed += Invalidate;
    }

    protected override CreateParams CreateParams
    {
        get
        {
            var cp = base.CreateParams;
            cp.ExStyle |= WS_EX_TOPMOST | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE;
            return cp;
        }
    }

    protected override bool ShowWithoutActivation => true;

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        RegisterHotKey(Handle, ESC_HOTKEY_ID, 0x0000, VK_ESCAPE);
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WM_MOUSEACTIVATE)
        {
            m.Result = (IntPtr)MA_NOACTIVATE;
            return;
        }

        if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == ESC_HOTKEY_ID)
        {
            _selection.Cancel();
            Close();
            return;
        }

        base.WndProc(ref m);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
            _selection.Start(e.Location);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        _selection.Update(e.Location);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            Hide();
            _selection.Complete();
            Close();
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        if (!_selection.IsSelecting || _selection.Bounds.IsEmpty) return;

        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var bounds = _selection.Bounds;

        // Relleno interno dinámico desde la entidad
        if (_selection.FillColor.A > 0)
        {
            using var fillBrush = new SolidBrush(_selection.FillColor);
            g.FillRectangle(fillBrush, bounds);
        }

        // Borde dinámico desde la entidad
        if (_selection.BorderColor.A > 0 && _selection.BorderWidth > 0)
        {
            using var borderPen = new Pen(_selection.BorderColor, _selection.BorderWidth)
            {
                DashStyle = _selection.BorderStyle
            };
            g.DrawRectangle(borderPen, bounds);
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        UnregisterHotKey(Handle, ESC_HOTKEY_ID);
        _selection.Changed -= Invalidate;
        base.OnFormClosing(e);
    }
}