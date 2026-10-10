using Engine.Assets.Icons;
using Engine.Entities.Commands;
using Engine.Entities.Commands.Extensions;
using Engine.Entities.PanelTriggers;
using Engine.Services.IconRenderer;
using Engine.Services.Network;

namespace Engine.Presentation.Windows;

public class FloatingTriggerForm : Form
{
    private const int WM_MOUSEACTIVATE = 0x0021;
    private const int MA_NOACTIVATE = 3;
    private const int WS_EX_TOPMOST = 0x00000008;
    private const int WS_EX_TOOLWINDOW = 0x00000080;
    private const int WS_EX_NOACTIVATE = 0x08000000;

    private readonly PanelTrigger _triggerModel;
    private readonly SendPromptCommand _sendPromptCommand;
    private readonly ProviderSelectionControl _providerControl;
    private readonly SystemPromptControl _systemPromptControl;
    private readonly DownloadExtensionCommand _downloadExtensionCommand;

    private readonly Panel _pnlDragHandle;
    private readonly Button _btnMainAction;
    private readonly Button _btnDropdown;
    private readonly NoActivateContextMenuStrip _contextMenu;

    public FloatingTriggerForm(
        PanelTrigger triggerModel,
        SendPromptCommand sendPromptCommand,
        ProviderSelectionControl providerControl,
        SystemPromptControl systemPromptControl,
        DownloadExtensionCommand downloadExtensionCommand)
    {
        _triggerModel = triggerModel ?? throw new ArgumentNullException(nameof(triggerModel));
        _sendPromptCommand = sendPromptCommand ?? throw new ArgumentNullException(nameof(sendPromptCommand));
        _providerControl = providerControl ?? throw new ArgumentNullException(nameof(providerControl));
        _systemPromptControl = systemPromptControl ?? throw new ArgumentNullException(nameof(systemPromptControl));
        _downloadExtensionCommand = downloadExtensionCommand ?? throw new ArgumentNullException(nameof(downloadExtensionCommand));

        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.Manual;
        TopMost = true;
        DoubleBuffered = true;

        Size = _triggerModel.Size;
        Location = _triggerModel.Position;
        BackColor = _triggerModel.BackColor;

        _pnlDragHandle = new Panel
        {
            Size = new Size(24, _triggerModel.Size.Height),
            Dock = DockStyle.Left,
            Cursor = Cursors.SizeAll,
            BackColor = _triggerModel.GripBackColor
        };
        _pnlDragHandle.Paint += (_, e) => e.Graphics.DrawGripTexture(_triggerModel.GripDotColor);
        _pnlDragHandle.MouseDown += (_, e) => { if (e.Button == MouseButtons.Left) _triggerModel.BeginDrag(e.Location); };
        _pnlDragHandle.MouseMove += (_, e) => { if (_triggerModel.IsDragging) _triggerModel.DragTo(PointToScreen(e.Location)); };
        _pnlDragHandle.MouseUp += (_, e) => { if (e.Button == MouseButtons.Left) _triggerModel.EndDrag(); };

        _contextMenu = BuildContextMenu();

        _btnMainAction = new Button
        {
            Dock = DockStyle.Fill,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        _btnMainAction.FlatAppearance.BorderSize = 0;
        _btnMainAction.FlatAppearance.MouseOverBackColor = _triggerModel.ButtonHoverColor;
        _btnMainAction.Paint += (_, e) => e.Graphics.DrawIcon(FoodIcons.Croissant, _btnMainAction.ClientRectangle, _triggerModel.ButtonForeColor);
        _sendPromptCommand.BindTo(_btnMainAction, parameterSupplier: () => new SendPromptArgs(Prompt: "Procesa la selección actual"));

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
        _btnDropdown.Click += (_, _) =>
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

    private NoActivateContextMenuStrip BuildContextMenu()
    {
        var menu = new NoActivateContextMenuStrip
        {
            ShowImageMargin = false,
            ShowCheckMargin = false,
            BackColor = _triggerModel.MenuBackColor,
            ForeColor = _triggerModel.MenuForeColor,
            Font = _triggerModel.Font,
            Padding = new Padding(2)
        };

        var localIp = IPService.GetLocalIPAddress();
        var ipItem = new ToolStripMenuItem($"🌐 IP: {localIp}");
        ipItem.Click += (_, _) =>
        {
            Clipboard.SetText(localIp);
        };
        menu.Items.Add(ipItem);

        menu.Items.Add(new ToolStripSeparator());

        menu.Items.Add(_sendPromptCommand.ToMenuItem(parameterSupplier: () => new SendPromptArgs(Prompt: "Procesa la selección actual")));
        
        menu.Items.Add(new ToolStripSeparator());

        var promptHost = new ToolStripControlHost(_systemPromptControl)
        {
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            AutoSize = false,
            Size = _systemPromptControl.Size
        };
        promptHost.Control.Click += (s, e) => { /* Interceptar para no cerrar */ };
        menu.Items.Add(promptHost);

        menu.Items.Add(new ToolStripSeparator());

        var providerHost = new ToolStripControlHost(_providerControl)
        {
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            AutoSize = false,
            Size = _providerControl.Size
        };
        providerHost.Control.Click += (s, e) => { /* Interceptar para no cerrar */ };
        menu.Items.Add(providerHost);

        menu.Items.Add(new ToolStripSeparator());

        menu.Items.Add(_downloadExtensionCommand.ToMenuItem());

        menu.Items.Add(new ToolStripSeparator());

        menu.Items.Add("❌ Ocultar Botón", null, (_, _) =>
        {
            Hide();
            _triggerModel.Hide();
        });

        menu.Opening += (_, _) =>
        {
            _systemPromptControl.RefreshPromptText();
            ipItem.Text = $"🌐 IP: {IPService.GetLocalIPAddress()}";
        };

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
            Invoke(action);
        else
            action();
    }
}

/// <summary>
/// Un menú contextual personalizado que rechaza el foco y las activaciones de ventana,
/// volviéndolo indetectable para aplicaciones que monitorean la pérdida de foco.
/// </summary>
/// <summary>
/// Un menú contextual personalizado que rechaza la activación de ventana y el robo de foco,
/// volviéndolo indetectable para aplicaciones que monitorean la pérdida de foco.
/// </summary>
public class NoActivateContextMenuStrip : ContextMenuStrip
{
    private const int WM_MOUSEACTIVATE = 0x0021;
    private const int MA_NOACTIVATE = 3;
    private const int WS_EX_NOACTIVATE = 0x08000000;

    protected override CreateParams CreateParams
    {
        get
        {
            CreateParams cp = base.CreateParams;
            cp.ExStyle |= WS_EX_NOACTIVATE;
            return cp;
        }
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WM_MOUSEACTIVATE)
        {
            m.Result = (IntPtr)MA_NOACTIVATE;
            return;
        }
        base.WndProc(ref m);
    }
}