using Engine.Assets.Icons;
using Engine.Entities.PanelTriggers;
using Engine.Services.IconRenderer;

namespace Engine.Presentation.Windows;

public class FloatingTriggerForm : Form
{
    private const int WM_MOUSEACTIVATE = 0x0021;
    private const int MA_NOACTIVATE = 3;

    private const int WS_EX_TOPMOST = 0x00000008;
    private const int WS_EX_TOOLWINDOW = 0x00000080;
    private const int WS_EX_NOACTIVATE = 0x08000000;

    private readonly PanelTrigger _triggerModel;
    private readonly Panel _pnlDragHandle;
    private readonly Button _btnMainAction;
    private readonly Button _btnDropdown;
    private readonly ContextMenuStrip _contextMenu;

    public event Action<string>? OnActionExecuted;

    public FloatingTriggerForm(PanelTrigger triggerModel)
    {
        _triggerModel = triggerModel ?? throw new ArgumentNullException(nameof(triggerModel));

        // Configuración de la Ventana
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.Manual;
        TopMost = true;
        DoubleBuffered = true;

        Size = _triggerModel.Size;
        Location = _triggerModel.Position;
        BackColor = _triggerModel.BackColor;

        // 1. Grip de Arrastre
        _pnlDragHandle = new Panel
        {
            Size = new Size(24, _triggerModel.Size.Height),
            Dock = DockStyle.Left,
            Cursor = Cursors.SizeAll,
            BackColor = _triggerModel.GripBackColor
        };
        _pnlDragHandle.Paint += (s, e) => e.Graphics.DrawGripTexture(_triggerModel.GripDotColor);
        _pnlDragHandle.MouseDown += (s, e) =>
        {
            if (e.Button == MouseButtons.Left) _triggerModel.BeginDrag(e.Location);
        };
        _pnlDragHandle.MouseMove += (s, e) =>
        {
            if (_triggerModel.IsDragging) _triggerModel.DragTo(PointToScreen(e.Location));
        };
        _pnlDragHandle.MouseUp += (s, e) =>
        {
            if (e.Button == MouseButtons.Left) _triggerModel.EndDrag();
        };

        // 2. Menú Desplegable
        _contextMenu = BuildContextMenu();

        // 3. Botón de Acción Principal (Renderizado en una sola línea)
        _btnMainAction = new Button
        {
            Dock = DockStyle.Fill,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        _btnMainAction.FlatAppearance.BorderSize = 0;
        _btnMainAction.FlatAppearance.MouseOverBackColor = _triggerModel.ButtonHoverColor;
        _btnMainAction.Paint += (s, e) =>
            e.Graphics.DrawIcon(FoodIcons.Croissant, _btnMainAction.ClientRectangle, _triggerModel.ButtonForeColor);
        _btnMainAction.Click += (s, e) => ExecuteAction("SEND_PROMPT");

        // 4. Botón Dropdown
        _btnDropdown = new Button
        {
            Text = "▼",
            Size = new Size(26, _triggerModel.Size.Height),
            Dock = DockStyle.Right,
            FlatStyle = FlatStyle.Flat,
            ForeColor = _triggerModel.DropdownForeColor,
            Font = _triggerModel.DropdownFont,
            Cursor = Cursors.Hand
        };
        _btnDropdown.FlatAppearance.BorderSize = 0;
        _btnDropdown.FlatAppearance.MouseOverBackColor = _triggerModel.DropdownHoverColor;
        _btnDropdown.Click += (s, e) =>
        {
            _contextMenu.Show(_btnDropdown, new Point(0, _btnDropdown.Height));
            _triggerModel.Open();
        };

        Controls.Add(_btnMainAction);
        Controls.Add(_btnDropdown);
        Controls.Add(_pnlDragHandle);

        SubscribeToModelEvents();
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

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WM_MOUSEACTIVATE)
        {
            m.Result = (IntPtr)MA_NOACTIVATE;
            return;
        }

        base.WndProc(ref m);
    }

    public void ExecuteAction(string actionKey)
    {
        _triggerModel.Click();
        OnActionExecuted?.Invoke(actionKey);
    }

    private ContextMenuStrip BuildContextMenu()
    {
        var menu = new ContextMenuStrip
        {
            ShowImageMargin = false,
            BackColor = _triggerModel.MenuBackColor,
            ForeColor = _triggerModel.MenuForeColor,
            Font = _triggerModel.Font
        };

        menu.Items.Add("💬 Enviar Prompt", null, (s, e) => ExecuteAction("SEND_PROMPT"));
        menu.Items.Add("📷 Capturar Pantalla", null, (s, e) => ExecuteAction("CAPTURE_SCREEN"));
        menu.Items.Add("🚫 Cancelar Operación", null, (s, e) => ExecuteAction("CANCEL"));
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("❌ Ocultar Botón", null, (s, e) =>
        {
            Hide();
            _triggerModel.Hide();
        });

        return menu;
    }

    private void SubscribeToModelEvents()
    {
        _triggerModel.PositionChanged += pos => SafeInvoke(() => Location = pos);
        _triggerModel.VisibilityChanged += visible => SafeInvoke(() => Visible = visible);
        _triggerModel.StyleChanged += () => SafeInvoke(() =>
        {
            Size = _triggerModel.Size;
            BackColor = _triggerModel.BackColor;
            _pnlDragHandle.BackColor = _triggerModel.GripBackColor;
            _btnMainAction.FlatAppearance.MouseOverBackColor = _triggerModel.ButtonHoverColor;
            _btnDropdown.ForeColor = _triggerModel.DropdownForeColor;
            _btnDropdown.Font = _triggerModel.DropdownFont;
            Invalidate(true);
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