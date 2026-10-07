using System.Runtime.InteropServices;
using Engine.Entities.Selections;

namespace Presentation.Windows;

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
        BackColor = Color.Black;
        Opacity = 0.05; 

        _selection.Changed += Invalidate;
    }

    protected override CreateParams CreateParams
    {
        get
        {
            CreateParams cp = base.CreateParams;
            cp.ExStyle |= WS_EX_TOPMOST | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE;
            return cp;
        }
    }

    protected override bool ShowWithoutActivation => true;

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        // Registrar ESC para cancelar la selección
        RegisterHotKey(Handle, ESC_HOTKEY_ID, 0x0000, VK_ESCAPE);
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WM_MOUSEACTIVATE)
        {
            m.Result = (IntPtr)MA_NOACTIVATE;
            return;
        }

        // Interceptar la tecla ESC para cerrar y cancelar
        if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == ESC_HOTKEY_ID)
        {
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
            _selection.Complete();
            Close();
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        if (!_selection.IsSelecting) return;

        using var pen = new Pen(Color.FromArgb(220, 255, 0, 0), 2);
        e.Graphics.DrawRectangle(pen, _selection.Bounds);
        
        using var brush = new SolidBrush(Color.FromArgb(10, 255, 0, 0));
        e.Graphics.FillRectangle(brush, _selection.Bounds);
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        UnregisterHotKey(Handle, ESC_HOTKEY_ID);
        _selection.Changed -= Invalidate;
        base.OnFormClosing(e);
    }
}