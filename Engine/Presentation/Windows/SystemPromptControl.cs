using Engine.Entities.Commands;
using Engine.Services.Prompt;

namespace Engine.Presentation.Windows;

public class SystemPromptControl : UserControl
{
    private readonly SystemPromptService _promptService;
    private readonly SetSystemPromptCommand _setCommand;

    private readonly Label _lblTitle;
    private readonly TextBox _txtPrompt;
    private readonly Button _btnSave;

    public SystemPromptControl(
        SystemPromptService promptService,
        SetSystemPromptCommand setCommand)
    {
        _promptService = promptService ?? throw new ArgumentNullException(nameof(promptService));
        _setCommand = setCommand ?? throw new ArgumentNullException(nameof(setCommand));

        Size = new Size(240, 110); // Ligeramente más compacto
        Padding = new Padding(8, 4, 8, 4); // Espaciado lateral para respirar
        BackColor = Color.Transparent; // Hereda el color oscuro del menú
        ForeColor = Color.White;

        // Título del control
        _lblTitle = new Label
        {
            Text = "⚙️ System Prompt Persistente",
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            Dock = DockStyle.Top,
            Height = 20, // Más pegado
            ForeColor = Color.LightGray,
            Padding = new Padding(0, 0, 0, 2)
        };

        // Botón Guardar
        _btnSave = new Button
        {
            Text = "Guardar",
            Font = new Font("Segoe UI", 8F, FontStyle.Bold),
            Dock = DockStyle.Bottom,
            Height = 24, // Botón más delgado
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(0, 122, 204),
            ForeColor = Color.White,
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 4, 0, 0)
        };
        _btnSave.FlatAppearance.BorderSize = 0;

        // Campo de texto multilínea
        _txtPrompt = new TextBox
        {
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(45, 45, 48), // Gris oscuro acorde al tema
            ForeColor = Color.White,
            BorderStyle = BorderStyle.None, // Sin borde tosco de Windows
            Font = new Font("Segoe UI", 8.5F),
            Text = _promptService.CurrentSystemPrompt
        };

        _btnSave.Click += async (_, _) =>
        {
            if (_setCommand.CanExecute(_txtPrompt.Text))
            {
                await _setCommand.ExecuteAsync(_txtPrompt.Text);

                _btnSave.Text = "✓ Guardado";
                _btnSave.BackColor = Color.FromArgb(40, 167, 69);
                await Task.Delay(1200);

                if (!IsDisposed)
                {
                    _btnSave.Text = "Guardar";
                    _btnSave.BackColor = Color.FromArgb(0, 122, 204);
                }
            }
        };

        Controls.Add(_txtPrompt);
        Controls.Add(_btnSave);
        Controls.Add(_lblTitle);
    }

    public void RefreshPromptText()
    {
        _txtPrompt.Text = _promptService.CurrentSystemPrompt;
    }
}