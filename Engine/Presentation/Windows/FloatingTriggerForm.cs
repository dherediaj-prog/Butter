using System.Drawing.Drawing2D;
using Engine.Entities.PanelTriggers;

namespace Engine.Presentation.Windows;

public class FloatingTriggerForm : Form
{
    // --- Win32 API para no activar el foco de la ventana ---
    private const int WM_MOUSEACTIVATE = 0x0021;
    private const int MA_NOACTIVATE = 3;

    private const int WS_EX_TOPMOST = 0x00000008;
    private const int WS_EX_TOOLWINDOW = 0x00000080;
    private const int WS_EX_NOACTIVATE = 0x08000000;

    // --- Controles de UI ---
    private readonly Panel _pnlDragHandle;
    private readonly Button _btnMainAction;
    private readonly Button _btnDropdown;
    private readonly ContextMenuStrip _contextMenu;

    // --- Estado para el Arrastre (Sin robar foco) ---
    private bool _isDragging;
    private Point _dragOffset;

    // --- Entidad opcional de dominio ---
    private readonly PanelTrigger? _triggerModel;

    // --- Eventos públicos para futuras integraciones ---
    public event Action<string>? OnActionExecuted;

    public FloatingTriggerForm(PanelTrigger? triggerModel = null)
    {
        _triggerModel = triggerModel;

        // Configuración básica del Form
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.Manual;
        TopMost = true;
        Size = new Size(180, 36);
        BackColor = Color.FromArgb(32, 33, 36); // Gris oscuro moderno
        DoubleBuffered = true;

        if (_triggerModel is not null)
        {
            Location = _triggerModel.Position;
        }
        else
        {
            Location = new Point(200, 200);
        }

        // 1. Inicialización del Agarrador de Arrastre (Grip Bar)
        _pnlDragHandle = new Panel
        {
            Size = new Size(24, 36),
            Dock = DockStyle.Left,
            Cursor = Cursors.SizeAll,
            BackColor = Color.FromArgb(45, 48, 53)
        };
        _pnlDragHandle.Paint += DrawDragGripTexture;
        _pnlDragHandle.MouseDown += DragHandle_MouseDown;
        _pnlDragHandle.MouseMove += DragHandle_MouseMove;
        _pnlDragHandle.MouseUp += DragHandle_MouseUp;

        // 2. Menú Desplegable (Hardcodeado y personalizable)
        _contextMenu = BuildContextMenu();

        // 3. Botón de Acción Principal
        _btnMainAction = new Button
        {
            Text = "🤖 Acciones IA",
            Dock = DockStyle.Fill,
            FlatStyle = FlatStyle.Flat,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            Cursor = Cursors.Hand,
            TextAlign = ContentAlignment.MiddleCenter
        };
        _btnMainAction.FlatAppearance.BorderSize = 0;
        _btnMainAction.FlatAppearance.MouseOverBackColor = Color.FromArgb(55, 58, 64);
        _btnMainAction.Click += (s, e) => ExecuteDefaultAction();

        // 4. Botón Flecha Dropdown
        _btnDropdown = new Button
        {
            Text = "▼",
            Size = new Size(26, 36),
            Dock = DockStyle.Right,
            FlatStyle = FlatStyle.Flat,
            ForeColor = Color.Gainsboro,
            Font = new Font("Segoe UI", 7f, FontStyle.Regular),
            Cursor = Cursors.Hand
        };
        _btnDropdown.FlatAppearance.BorderSize = 0;
        _btnDropdown.FlatAppearance.MouseOverBackColor = Color.FromArgb(65, 68, 75);
        _btnDropdown.Click += (s, e) => ShowDropdownMenu();

        // Ensamblado del Layout
        Controls.Add(_btnMainAction);
        Controls.Add(_btnDropdown);
        Controls.Add(_pnlDragHandle);

        SubscribeToModelEvents();
    }

    #region Window Focus & Non-Activation Logic

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
        // Previene la activación de la ventana cuando el usuario hace clic en el cuerpo/grip
        if (m.Msg == WM_MOUSEACTIVATE)
        {
            m.Result = (IntPtr)MA_NOACTIVATE;
            return;
        }
        base.WndProc(ref m);
    }

    #endregion

    #region Drag Logic (Manual Delta Position)

    private void DragHandle_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            _isDragging = true;
            _dragOffset = e.Location;
        }
    }

    private void DragHandle_MouseMove(object? sender, MouseEventArgs e)
    {
        if (!_isDragging) return;

        // Calcula la nueva posición en coordenadas globales del monitor
        var newScreenPos = PointToScreen(e.Location);
        var targetLocation = new Point(
            newScreenPos.X - _dragOffset.X,
            newScreenPos.Y - _dragOffset.Y
        );

        Location = targetLocation;

        // Actualiza el modelo de dominio si existe
        _triggerModel?.SetPosition(targetLocation);
    }

    private void DragHandle_MouseUp(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            _isDragging = false;
        }
    }

    private static void DrawDragGripTexture(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        using var brush = new SolidBrush(Color.FromArgb(120, 130, 140));

        // Dibuja 6 puntos verticales estilizados como textura Grip
        int startX = 8;
        int startY = 10;
        for (int i = 0; i < 3; i++)
        {
            g.FillEllipse(brush, startX, startY + (i * 6), 3, 3);
            g.FillEllipse(brush, startX + 5, startY + (i * 6), 3, 3);
        }
    }

    #endregion

    #region Dropdown Menu & Actions

    private ContextMenuStrip BuildContextMenu()
    {
        var menu = new ContextMenuStrip
        {
            ShowImageMargin = false,
            BackColor = Color.FromArgb(40, 42, 46),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9f)
        };

        // Opciones Hardcodeadas
        menu.Items.Add("💬 Enviar Prompt", null, (s, e) => ExecuteAction("SEND_PROMPT"));
        menu.Items.Add("📷 Capturar Pantalla", null, (s, e) => ExecuteAction("CAPTURE_SCREEN"));
        menu.Items.Add("🚫 Cancelar Operación", null, (s, e) => ExecuteAction("CANCEL"));
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("❌ Ocultar Botón", null, (s, e) => HideForm());

        return menu;
    }

    private void ShowDropdownMenu()
    {
        _contextMenu.Show(_btnDropdown, new Point(0, _btnDropdown.Height));
        _triggerModel?.Open();
    }

    /// <summary>
    /// Acción predeterminada al hacer clic directamente en la parte principal del botón.
    /// </summary>
    public virtual void ExecuteDefaultAction()
    {
        ExecuteAction("SEND_PROMPT");
    }

    /// <summary>
    /// Método preparado para la ejecución centralizada y despacho de comandos.
    /// </summary>
    public virtual void ExecuteAction(string actionKey)
    {
        _triggerModel?.Click();
        OnActionExecuted?.Invoke(actionKey);

        switch (actionKey)
        {
            case "SEND_PROMPT":
                // Futura implementación: invocar SendPromptCommand
                break;

            case "CAPTURE_SCREEN":
                // Futura implementación: invocar captura de pantalla
                break;

            case "CANCEL":
                // Futura implementación: invocar SendCancelCommand
                break;
        }
    }

    public void HideForm()
    {
        Hide();
        _triggerModel?.Hide();
    }

    #endregion

    #region Synchronization with Engine Model

    private void SubscribeToModelEvents()
    {
        if (_triggerModel is null) return;

        _triggerModel.PositionChanged += pos => SafeInvoke(() => Location = pos);
        _triggerModel.VisibilityChanged += visible => SafeInvoke(() => Visible = visible);
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
        if (_triggerModel is not null)
        {
            _triggerModel.PositionChanged -= pos => { };
            _triggerModel.VisibilityChanged -= visible => { };
        }
        base.OnFormClosing(e);
    }

    #endregion
}