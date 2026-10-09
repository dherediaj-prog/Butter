using Engine.Entities.Network.ActiveAIProviderSelectors;
using Engine.Entities.Network.Provider;

namespace Engine.Presentation.Windows;

public class ProviderSelectionControl : UserControl
{
    private readonly AIProviderManager _manager;
    private readonly ActiveAIProviderSelector _selector;
    private readonly FlowLayoutPanel _layoutPanel;

    public ProviderSelectionControl(AIProviderManager manager, ActiveAIProviderSelector selector)
    {
        _manager = manager ?? throw new ArgumentNullException(nameof(manager));
        _selector = selector ?? throw new ArgumentNullException(nameof(selector));

        // Configuración visual del contenedor
        BackColor = Color.FromArgb(40, 42, 46);
        ForeColor = Color.White;
        Size = new Size(220, 160);
        Padding = new Padding(4);
        DoubleBuffered = true;

        _layoutPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            BackColor = Color.Transparent,
            Margin = Padding.Empty
        };

        Controls.Add(_layoutPanel);

        // Suscripción a eventos de los gestores
        _manager.OnProviderRegistered += OnProvidersStateChanged;
        _manager.OnProviderUnregistered += OnProvidersStateChanged;
        _selector.OnActiveProviderChanged += OnActiveProviderChanged;

        // Renderizado inicial
        RenderProviders();
    }

    private void OnProvidersStateChanged(AIProvider _) => SafeInvoke(RenderProviders);

    private void OnActiveProviderChanged(AIProvider? _) => SafeInvoke(RenderProviders);

    private void RenderProviders()
    {
        _layoutPanel.SuspendLayout();

        // Limpieza segura de los controles anteriores
        foreach (Control control in _layoutPanel.Controls)
        {
            control.Dispose();
        }

        _layoutPanel.Controls.Clear();

        var connectedProviders = _manager.GetConnected().ToList();
        var activeProvider = _selector.ActiveProvider;

        if (connectedProviders.Count == 0)
        {
            _layoutPanel.Controls.Add(new Label
            {
                Text = "No hay IAs conectadas.",
                ForeColor = Color.Gray,
                AutoSize = true,
                Margin = new Padding(5, 10, 5, 0)
            });
        }
        else
        {
            foreach (var provider in connectedProviders)
            {
                bool isActive = provider == activeProvider;

                var btnProvider = new Button
                {
                    Text = provider.Metadata?.Provider ?? $"Provider {provider.Id[..6]}",
                    Width = _layoutPanel.Width - 24, // Ajuste para el ScrollBar
                    Height = 32,
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand,
                    BackColor = isActive ? Color.FromArgb(65, 68, 75) : Color.FromArgb(45, 48, 53),
                    ForeColor = isActive ? Color.White : Color.Gainsboro,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Font = new Font("Segoe UI", 9f, isActive ? FontStyle.Bold : FontStyle.Regular),
                    Margin = new Padding(2)
                };

                btnProvider.FlatAppearance.BorderSize = 0;
                btnProvider.FlatAppearance.MouseOverBackColor = Color.FromArgb(75, 78, 85);

                // Acción de selección al hacer clic
                btnProvider.Click += (_, _) => _selector.Select(provider);

                _layoutPanel.Controls.Add(btnProvider);
            }
        }

        _layoutPanel.ResumeLayout();
    }

    private void SafeInvoke(Action action)
    {
        if (IsDisposed || !IsHandleCreated) return;
        if (InvokeRequired)
            BeginInvoke(action);
        else
            action();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _manager.OnProviderRegistered -= OnProvidersStateChanged;
            _manager.OnProviderUnregistered -= OnProvidersStateChanged;
            _selector.OnActiveProviderChanged -= OnActiveProviderChanged;
        }

        base.Dispose(disposing);
    }
}