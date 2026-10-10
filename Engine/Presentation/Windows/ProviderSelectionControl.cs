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

        BackColor = Color.Transparent; // Integración limpia con el menú
        ForeColor = Color.White;
        Size = new Size(240, 90); // Más ancho (igual que SystemPrompt) y más bajo
        Padding = new Padding(6, 2, 6, 2);
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

        _manager.OnProviderRegistered += OnProvidersStateChanged;
        _manager.OnProviderUnregistered += OnProvidersStateChanged;
        _selector.OnActiveProviderChanged += OnActiveProviderChanged;

        RenderProviders();
    }

    private void OnProvidersStateChanged(AIProvider _) => SafeInvoke(RenderProviders);

    private void OnActiveProviderChanged(AIProvider? _) => SafeInvoke(RenderProviders);

    private void RenderProviders()
    {
        _layoutPanel.SuspendLayout();

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
                Margin = new Padding(5, 5, 5, 0)
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
                    Width = _layoutPanel.Width - 18, // Ajuste para el ScrollBar y padding
                    Height = 28, // Botones más compactos
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand,
                    BackColor = isActive ? Color.FromArgb(65, 68, 75) : Color.FromArgb(45, 48, 53),
                    ForeColor = isActive ? Color.White : Color.Gainsboro,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Font = new Font("Segoe UI", 8.5f, isActive ? FontStyle.Bold : FontStyle.Regular),
                    Margin = new Padding(1, 1, 1, 3) // Separación leve
                };

                btnProvider.FlatAppearance.BorderSize = 0;
                btnProvider.FlatAppearance.MouseOverBackColor = Color.FromArgb(75, 78, 85);

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