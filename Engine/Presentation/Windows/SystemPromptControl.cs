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

        Size = new Size(240, 115);
        Padding = new Padding(6);
        BackColor = Color.FromArgb(32, 32, 32);
        ForeColor = Color.White;

        // Título del control
        _lblTitle = new Label
        {
            Text = "⚙️ System Prompt Persistente",
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            Dock = DockStyle.Top,
            Height = 22,
            ForeColor = Color.LightGray
        };

        // Botón Guardar
        _btnSave = new Button
        {
            Text = "Guardar Prompt",
            Font = new Font("Segoe UI", 8F, FontStyle.Bold),
            Dock = DockStyle.Bottom,
            Height = 26,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(0, 122, 204),
            ForeColor = Color.White,
            Cursor = Cursors.Hand
        };
        _btnSave.FlatAppearance.BorderSize = 0;

        // Campo de texto multilínea para el prompt
        _txtPrompt = new TextBox
        {
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(45, 45, 48),
            ForeColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            Font = new Font("Segoe UI", 8.5F),
            Text = _promptService.CurrentSystemPrompt
        };

        // Evento para guardar de forma asíncrona
        _btnSave.Click += async (_, _) =>
        {
            if (_setCommand.CanExecute(_txtPrompt.Text))
            {
                await _setCommand.ExecuteAsync(_txtPrompt.Text);

                // Feedback visual de guardado exitoso
                _btnSave.Text = "✓ Guardado";
                _btnSave.BackColor = Color.FromArgb(40, 167, 69);
                await Task.Delay(1200);

                if (!IsDisposed)
                {
                    _btnSave.Text = "Guardar Prompt";
                    _btnSave.BackColor = Color.FromArgb(0, 122, 204);
                }
            }
        };

        Controls.Add(_txtPrompt);
        Controls.Add(_btnSave);
        Controls.Add(_lblTitle);
    }

    /// <summary>
    /// Refresca el contenido del campo de texto con el prompt persistente actual.
    /// </summary>
    public void RefreshPromptText()
    {
        _txtPrompt.Text = _promptService.CurrentSystemPrompt;
    }
}